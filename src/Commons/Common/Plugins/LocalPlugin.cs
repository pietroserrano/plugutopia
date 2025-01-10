using System.Reflection;
using Common.Model.Abstractions;
using Common.Model.Models;

namespace Common;

public class LocalPlugin : IPlugin
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public PluginManifest Manifest { get; private set; }
    public Assembly? Assembly { get; private set; }
    public IPluginRegistrator? Registrator { get; private set; }
    public bool IsInError { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string Directory { get; private set; }

    public LocalPlugin(PluginManifest manifest, string directory, Assembly? assembly = null, IPluginRegistrator? registrator = null, bool isInError = false, string errorMessage = null)
    {
        Id = manifest.Id;
        Name = manifest.Name;
        Assembly = assembly;
        Registrator = registrator;
        Manifest = manifest;
        IsInError = isInError;
        ErrorMessage = errorMessage;
        Directory = directory;
    }

    public void SetAssembly(Assembly assembly) => Assembly = assembly;
    public void SetPluginRegistrator(IPluginRegistrator registrator) => Registrator = registrator;
    public void SetErrorDetails(string? errorMessage)
    {
        IsInError = true;
        ErrorMessage = errorMessage;
    }
}