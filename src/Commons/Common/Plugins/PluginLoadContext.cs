using Common.Model.Models;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace Common;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoadContext"/> class.
    /// </summary>
    /// <param name="path">The path of the plugin assembly.</param>
    public PluginLoadContext(string path) : base(true)
    {
        _resolver = new AssemblyDependencyResolver(path);
        _path = path;
    }

    /// <inheritdoc />
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    public Assembly LoadPlugin(PluginManifest manifest)
    {
        var _assemblyToExludes = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.ManifestModule.Name).ToList();
        var assembly = LoadFromAssemblyPath(Path.Combine(_path, $"{manifest.AssemblyName}.dll"));
        
        var dllFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(x => !_assemblyToExludes.Contains(x))
            .ToList();
        _assemblyToExludes.AddRange(dllFiles);

        foreach (var refAssembly in manifest.ReferencedAssemblies.Where(a => !_assemblyToExludes.Contains(a)))
        {
            var ass = LoadFromAssemblyPath(Path.Combine(_path, refAssembly));
            //ass.GetTypes();
        }

        return assembly;
    }
}

#region old implementation
//class PluginLoadContext : AssemblyLoadContext
//{
//    private AssemblyDependencyResolver _resolver;
//    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
//    {
//        _resolver = new AssemblyDependencyResolver(pluginPath);
//    }

//    protected override Assembly Load(AssemblyName assemblyName)
//    {

//        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
//        if (assemblyPath != null)
//        {
//            return LoadFromAssemblyPath(assemblyPath);
//            // return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
//        }

//        return null;
//    }

//    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
//    {
//        string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
//        if (libraryPath != null)
//        {
//            return LoadUnmanagedDllFromPath(libraryPath);
//        }

//        return IntPtr.Zero;
//    }

//    public static Assembly LoadPlugin(PluginManifest manifest, string directory)
//    {
//        // string root = Path.GetFullPath(directory);

//        //TODO: Dipendenze native?
//        // RegisterProjectDIExtensions.LoadNativeLibrariesFromPluginDirectory(root);
//        var _assemblyToExludes = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.ManifestModule.Name).ToList();

//        PluginLoadContext loadContext = new PluginLoadContext(directory);
//        var assembly =  loadContext.LoadFromAssemblyPath(Path.Combine(directory, $"{manifest.AssemblyName}.dll"));
//        // Trova tutti i file .dll nella directory corrente
//        var dllFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
//            .Select(Path.GetFileName)
//            .Where(x=> !_assemblyToExludes.Contains(x))
//            .ToList();
//        _assemblyToExludes.AddRange(dllFiles);

//        foreach (var refAssembly in manifest.ReferencedAssemblies.Where(a => !_assemblyToExludes.Contains(a)))
//        {
//            var ass = loadContext.LoadFromAssemblyPath(Path.Combine(directory, refAssembly));
//            //ass.GetTypes();
//        }

//        return assembly;
//    }

//    private static IEnumerable<string> GetDllFilesFromMetadata(string metadataFilePath)
//    {
//        var path = Path.GetDirectoryName(metadataFilePath);

//        using (var stream = File.OpenRead(metadataFilePath))
//        {
//            var metadata = JsonSerializer.Deserialize<PluginManifest>(stream);
//            return metadata?.ReferencedAssemblies.Select(f => Path.Combine(path, f)) ?? Enumerable.Empty<string>();
//        }
//    }
//}
#endregion