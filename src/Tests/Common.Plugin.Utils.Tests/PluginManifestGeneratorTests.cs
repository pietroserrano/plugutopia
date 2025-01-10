using Common.Model.Models;
using System.Text.Json;

namespace Common.Plugin.Utils.Tests;

public class PluginManifestGeneratorTests
{
    private static readonly string _testPathRoot = Path.Combine(Path.GetTempPath(), "pmp-test-data");
    private string _pluginFolder = string.Empty;
    private string _tempPath = string.Empty;

    public PluginManifestGeneratorTests()
    {
        (_tempPath, _pluginFolder) = GetTestPaths("plugin-" + Path.GetRandomFileName());
        Directory.CreateDirectory(_pluginFolder);
    }
    [Fact]
    public void GeneratePluginManifest_Succesfully()
    {
        Assembly assembly = this.GetType().Assembly;

        PluginManifest internalManifest = new()
        {
            Category = "TestPluginForPlanet",
            Changelog ="This is a changelog",
            Description="This is a description for bho",
            Name="TravoltaTest",
            Overview="This is an Overview",
            Owner= "John Travolta",
            Id=Guid.NewGuid()
        };

        var manifestString = PluginManifestHelper.PopulatePluginManifest(internalManifest, assembly);

        var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestString);

        Assert.Equal(assembly.GetName().Name, manifest.AssemblyName);
        Assert.NotEmpty(manifest.ReferencedAssemblies);
        Assert.Contains("Common.Plugin.dll", manifest.ReferencedAssemblies);
        Assert.Equal(assembly.GetName().Version.ToString(), manifest.Version);
        Assert.Equal(manifest.Timestamp.Date, DateTime.Now.Date);

        Assert.NotEqual("0.0.1", manifest.TargetAbi);

        Assert.Equal(internalManifest.Category, manifest.Category);
        Assert.Equal(internalManifest.Name, manifest.Name);
        Assert.Equal(internalManifest.Description, manifest.Description);
        Assert.Equal(internalManifest.Changelog, manifest.Changelog);
        Assert.Equal(internalManifest.Overview, manifest.Overview);
        Assert.Equal(internalManifest.Owner, manifest.Owner);
        Assert.Equal(internalManifest.Id, manifest.Id);
    }

    [Fact]
    public void SetTargetAbi_ReturnDefaultVersion_IfPluginDllNotFound()
    {
        var manifest = new PluginManifest();
        //dll ref not present in ReferencedAssemblies
        PluginManifestHelper.SetTargetAbi("", manifest);
        Assert.Equal(PluginManifestHelper.DEFAULT_TARGETABI_VERSION, manifest.TargetAbi);

        //add lib in ReferencedAssemblies manually
        manifest.ReferencedAssemblies.Add("Common.Plugin.dll");
        PluginManifestHelper.SetTargetAbi("", manifest);
        Assert.Equal(PluginManifestHelper.DEFAULT_TARGETABI_VERSION, manifest.TargetAbi);
    }

    private (string TempPath, string PluginFolder) GetTestPaths(string pluginFolderName)
    {
        var tempPath = Path.Combine(_testPathRoot, "plugin-manager" + Path.GetRandomFileName());
        var pluginFolder = Path.Combine(tempPath, pluginFolderName);

        return (tempPath, pluginFolder);
    }

}