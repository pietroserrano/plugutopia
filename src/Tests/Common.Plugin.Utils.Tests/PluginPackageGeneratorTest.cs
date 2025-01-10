using Common.Model.Models;
using System.Text.Json;

namespace Common.Plugin.Utils.Tests;
public class PluginPackageHelpersTests
{
    [Fact]
    public void GeneratePackage_ShouldThrowFileNotFoundException_WhenPluginManifestIsNullOrEmpty()
    {
        // Arrange
        string pluginManifest = string.Empty;
        string? destinationZipFile = null;

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => PluginPackageHelpers.GeneratePackage(pluginManifest, destinationZipFile));
    }

    [Fact]
    public void GeneratePackage_ShouldThrowFileNotFoundException_WhenPluginManifestDoesNotExist()
    {
        // Arrange
        string pluginManifest = "nonexistent.json";
        string? destinationZipFile = null;

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => PluginPackageHelpers.GeneratePackage(pluginManifest, destinationZipFile));
    }

    [Fact]
    public void GeneratePackage_ShouldThrowFileNotFoundException_WhenDestinationDoesNotExist()
    {
        // Arrange
        string pluginManifest = "validPluginManifest.json";
        string? destinationZipFile = "non_esiste";

        // Create a valid plugin manifest for testing
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0"
        };
        File.WriteAllText(pluginManifest, JsonSerializer.Serialize(manifest));

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => PluginPackageHelpers.GeneratePackage(pluginManifest, destinationZipFile));

        // Clean up
        File.Delete(pluginManifest);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("../..")]
    public void GeneratePackage_ShouldGenerateZipAndMd5Files_WhenPluginManifestIsValid(string? destinationZipFile)
    {
        // Arrange
        string pluginManifest = "validPluginManifest.json";

        // Create a valid plugin manifest for testing
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0"
        };
        File.WriteAllText(pluginManifest, JsonSerializer.Serialize(manifest));

        // Act
        PluginPackageHelpers.GeneratePackage(pluginManifest, destinationZipFile);

        // Assert
        if (destinationZipFile is null or "" or "non_esiste")
        {
            pluginManifest = Path.GetFullPath(pluginManifest);
            string tmpDestination = Path.GetDirectoryName(pluginManifest)!;

            destinationZipFile = destinationZipFile is null or ""
                ? Path.Combine(tmpDestination, "..")
                : tmpDestination;
        }
        destinationZipFile = Path.GetFullPath(destinationZipFile);
        string zipName = $"{manifest.Name}_{manifest.Version}.zip";
        string md5Name = $"{zipName}.md5.txt";
        string zipPath = Path.Combine(destinationZipFile, zipName);
        string md5Path = Path.Combine(destinationZipFile, md5Name);

        Assert.True(File.Exists(zipPath));
        Assert.True(File.Exists(md5Path));

        // Clean up
        File.Delete(pluginManifest);
        File.Delete(zipPath);
        File.Delete(md5Path);
    }
}
