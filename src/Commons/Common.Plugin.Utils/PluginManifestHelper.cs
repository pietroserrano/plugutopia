using System.Reflection;
using System.Text.Json;
using Common.Model.Models;

public static class PluginManifestHelper
{
    public static readonly string DEFAULT_TARGETABI_VERSION = "0.0.1";

    internal static void SetReferencedAssemblies(string assemblyDir, PluginManifest manifest)
    {
        manifest.ReferencedAssemblies = new List<string>();
        
        if (Directory.Exists(assemblyDir))
        {
            var assemblies = Directory.GetFiles(assemblyDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => file.EndsWith(".dll") || file.EndsWith(".exe"))
                .ToList();
            
            if (assemblies.Any())
            {
                manifest.ReferencedAssemblies.AddRange(assemblies
                    .Select(a => Path.GetFileName(a)!)
                    .ToList());
            }
        }
    }

    internal static Assembly LoadPluginAssemblyAndInternalManifest(string assemblyName, out PluginManifest manifest)
    {
        Assembly assembly = Assembly.LoadFile(assemblyName);
        
        string resourceName = $"{assembly.GetName().Name}.internal-manifest.json";
        manifest = new PluginManifest();
        
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assemblyName}'.");
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonString = reader.ReadToEnd();
                manifest = JsonSerializer.Deserialize<PluginManifest>(jsonString);
            }
        }

        return assembly;
    }

    internal static void SetTargetAbi(string assemblyDir, PluginManifest manifest)
    {
        manifest.TargetAbi = DEFAULT_TARGETABI_VERSION;
        var pluginAssembly = manifest.ReferencedAssemblies.FirstOrDefault(ra=> string.Equals(ra, "Common.Plugin.dll", StringComparison.InvariantCultureIgnoreCase));
        if (pluginAssembly == null) return;

        Assembly assembly;

        try
        {
            assembly = Assembly.LoadFile(Path.Combine(assemblyDir, pluginAssembly));
        }
        catch
        {
            return;
        }
        
        manifest.TargetAbi = assembly.GetName().Version?.ToString() ?? manifest.TargetAbi;
    }
    
    public static string GeneratePluginManifest(string assemblyName)
    {
        // Caricare l'assembly specificato
        var assembly = LoadPluginAssemblyAndInternalManifest(assemblyName, out var manifest);

        return PopulatePluginManifest(manifest, assembly);
    }

    internal static string PopulatePluginManifest(PluginManifest manifest, Assembly assembly)
    {
        var assemblyDir = Path.GetDirectoryName(assembly.Location)!;

        SetReferencedAssemblies(assemblyDir, manifest);

        manifest.Timestamp = DateTime.Now;
        manifest.Version = assembly.GetName().Version.ToString();
        manifest.AssemblyName = assembly.GetName().Name;

        SetTargetAbi(assemblyDir, manifest);

        return JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
    }
}