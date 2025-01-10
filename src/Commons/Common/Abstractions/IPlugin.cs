using System.Reflection;
using Common.Model.Models;

namespace Common.Model.Abstractions;

public interface IPlugin
{
    Guid Id { get; }
    string Name { get; }
    PluginManifest Manifest { get; }
    Assembly? Assembly { get; }
    IPluginRegistrator? Registrator { get; }
    bool IsInError { get; }
    string? ErrorMessage { get; }
    string Directory { get; }
}