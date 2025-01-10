using Common.Configurations;
using Common.Exceptions.Plugins;
using Common.Extensions;
using Common.Model.Abstractions;
using Common.Model.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace Common;

public class PluginManager : IPluginManager, IDisposable
{
    private IHost? _applicationHost;
    private readonly List<LocalPlugin> _plugins = new();
    private readonly List<PluginLoadContext> _pluginLoadContexts = new();
    private readonly ILogger<IPluginManager> _logger;
    private readonly IConfiguration _configuration;
    private readonly Version _appVersion;
    private readonly JsonSerializerOptions _jsonOptions;

    public PluginManager(ILogger<IPluginManager> logger,IConfiguration configuration, Version? appVersion = null)
    {
        _logger = logger;
        _configuration = configuration;
        _appVersion = appVersion == null ? AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, "Common.Plugin.dll"))?.GetName().Version ?? new Version("1.0.0") : appVersion;
        
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        
        _plugins = DiscoverPlugins().ToList();
    }

    public void RegisterServices(IServiceCollection serviceCollection)
    {
        foreach(var plugin in _plugins.Where(p=>p.Manifest.Status == PluginStatus.ENABLED))
        {
            plugin.Registrator!.AddServices(serviceCollection, _configuration);
        }
    }

    public IPluginManager LoadPlugin()
    {

        // Attempt to remove any deleted plugins and change any successors to be active.
        for (int i = _plugins.Count - 1; i >= 0; i--)
        {
            var plugin = _plugins[i];
            if (plugin.Manifest.Status == PluginStatus.DELETED && DeletePlugin(plugin))
            {
                // See if there is another version, and if so make that active.
                Process(plugin);
            }
        }

        foreach (var plugin in _plugins)
        {
            var manifest = plugin.Manifest;
            var directory = plugin.Directory;

            try
            {
                if (!_appVersion.PluginVersionIsValid(manifest.TargetAbi))
                    throw new PluginNotSupportedException(manifest.TargetAbi, _appVersion.ToString(), manifest.Name);
                    
                var assContext = new PluginLoadContext(directory);
                var assembly = assContext.LoadPlugin(manifest);
                _pluginLoadContexts.Add(assContext);
                plugin.SetAssembly(assembly);

                var pluginRegistratorType = assembly.GetTypes().FirstOrDefault(t => t.GetInterface(nameof(IPluginRegistrator)) != null);

                if (pluginRegistratorType == null)
                    throw new PluginMalfunctionedException(manifest.Name, "pluginRegistratorType is null");

                var pluginRegistrator = Activator.CreateInstance(pluginRegistratorType) as IPluginRegistrator;
                if (pluginRegistrator == null)
                    throw new PluginMalfunctionedException(manifest.Name, "pluginRegistrator is null");

                plugin.SetPluginRegistrator(pluginRegistrator);

            }
            catch(PluginNotSupportedException ex)
            {
                _logger.LogError(ex, $"{ex.Message}");
                plugin.SetErrorDetails(ex.Message);

                ChangePluginState(plugin, PluginStatus.NOT_SUPPORTED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to load assembly with name '{manifest.AssemblyName}': {ex.Message}");
                plugin.SetErrorDetails(ex.Message);
                
                FailPlugin(plugin);
            }
        }

        return this;
    }

    private IEnumerable<LocalPlugin> DiscoverPlugins()
    {
        var plugins = new List<LocalPlugin>();

        PluginConfiguration pluginConfiguration = new();

        _configuration.GetSection(PluginConfiguration.SECTION)
            .Bind(pluginConfiguration);

        var pluginDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location),
            pluginConfiguration.Directory);

        if (!Directory.Exists(pluginDirectory))
        {
            return Enumerable.Empty<LocalPlugin>();
        }

        var directories = Directory.EnumerateDirectories(pluginDirectory, "*.*", SearchOption.TopDirectoryOnly);
        
        foreach ( var directory in directories)
        {

            plugins.Add(ReadManifest(directory));
        }



        return plugins;
    }

    private LocalPlugin ReadManifest(string directory)
    {
        var manifestFile = Path.Combine(directory, PluginManifest.FileName);
        PluginManifest? manifest = new PluginManifest
        {
            Status = PluginStatus.MALFUNCTIONED,
            Name = manifestFile,
        };

        if (File.Exists(manifestFile))
        {
            using (var stream = File.OpenRead(manifestFile))
            {
                
                bool isError = false;
                string errorMessage = "";
                try
                {
                    manifest = JsonSerializer.Deserialize<PluginManifest>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (manifest == null)
                        throw new Exception("Manifest is null after JsonSerializer.Deserialize");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error on deserialize manifest file '{manifestFile}': {ex.Message}");
                    errorMessage = ex.Message;
                    isError = true;
                }

                return new LocalPlugin(manifest, isInError: isError, errorMessage: errorMessage, directory: directory);
            }
        }

        _logger.LogWarning($"{PluginManifest.FileName} not found in directory '{directory}'");
        return new LocalPlugin(manifest, isInError: true, errorMessage: $"{PluginManifest.FileName} not found in directory '{directory}'", directory: directory);
    }

    public void SetApplicationHost(IHost applicationHost) { _applicationHost = applicationHost; }

    public List<IPlugin> GetPlugins() => _plugins.Cast<IPlugin>().ToList();

    public List<Assembly> GetPluginAssemblies() => _plugins.Where(p=> p.Assembly != null).Select(p=> p.Assembly!).ToList();

    public void UseApplications(IHost host)
    {
        foreach (var plugin in _plugins.Where(p=> p.Registrator != null))
            plugin.Registrator!.UseApplication(_applicationHost?.Services);
    }

    private bool SaveManifest(IPlugin plugin)
    {
        try
        {
            var data = JsonSerializer.Serialize(plugin.Manifest, _jsonOptions);
            File.WriteAllText(Path.Combine(plugin.Directory, PluginManifest.FileName), data);
            return true;
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Unable to save plugin manifest to path '{Path}'", plugin.Directory);
            return false;
        }
    }

    private bool ChangePluginState(IPlugin plugin, PluginStatus state)
    {
        if (plugin.Manifest.Status == state || string.IsNullOrEmpty(plugin.Directory))
        {
            // No need to save as the state hasn't changed.
            return true;
        }

        plugin.Manifest.Status = state;
        return SaveManifest(plugin);
    }

    private bool FailPlugin(IPlugin plugin)
    {
        if (plugin is null)
            return false;

        return ChangePluginState(plugin, PluginStatus.MALFUNCTIONED);
    }

    public bool EnablePlugin(IPlugin plugin)
    {
        if (plugin is null)
            return false;

        return ChangePluginState(plugin, PluginStatus.ENABLED);
    }

    public bool DisablePlugin(IPlugin plugin)
    {
        var res = false;

        if (plugin is null)
            return res;

        res = ChangePluginState(plugin, PluginStatus.DISABLED);
        Process(plugin);
        return res;
    }

    public bool RemovePlugin(IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        if (DeletePlugin(plugin))
        {
            Process(plugin);
            return true;
        }

        _logger.LogWarning("Unable to delete {Path}, so marking as deleteOnStartup.", plugin.Directory);
        // Unable to delete, so disable.
        if (ChangePluginState(plugin, PluginStatus.DELETED))
        {
            Process(plugin);
            return true;
        }

        return false;
    }

    private bool DeletePlugin(IPlugin plugin)
    {
        // Attempt a cleanup of old folders.
        try
        {
            Directory.Delete(plugin.Directory, true);
            _logger.LogDebug("Deleted {Path}", plugin.Directory);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return false;
        }

        return _plugins.Remove(_plugins.First(p=>p.Id == plugin.Id));
    }

    private void Process(IPlugin plugin)
    {
        plugin.Manifest.Status = PluginStatus.RESTART;
        plugin.Manifest.AutoUpdate = false;
    }

    public void Dispose()
    {
        foreach (var assemblyLoadContext in _pluginLoadContexts)
        {
            assemblyLoadContext.Unload();
        }
    }
}