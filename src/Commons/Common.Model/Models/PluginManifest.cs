using System.Text.Json.Serialization;

namespace Common.Model.Models;

public class PluginManifest
{
    public const string FileName = "manifest.json";

    /// <summary>
    /// Name of principal assembly
    /// </summary>
    [JsonPropertyName("assemblyName")]
    public string AssemblyName { get; set; } = default!;
    /// <summary>
    /// Gets or sets the Version number of the plugin.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = default!;
    /// <summary>
    /// Gets or sets the collection of assemblies that should be loaded.
    /// </summary>
    [JsonPropertyName("referencedAssemblies")]
    public List<string> ReferencedAssemblies { get; set; } = new();
    /// <summary>
    /// Gets or sets the category of the plugin.
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets the changelog information.
    /// </summary>
    [JsonPropertyName("changelog")]
    public string Changelog { get; set; }

    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the Global Unique Identifier for the plugin.
    /// </summary>
    [JsonPropertyName("guid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Name of the plugin.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets an overview of the plugin.
    /// </summary>
    [JsonPropertyName("overview")]
    public string Overview { get; set; }

    /// <summary>
    /// Gets or sets the owner of the plugin.
    /// </summary>
    [JsonPropertyName("owner")]
    public string Owner { get; set; }

    /// <summary>
    /// Gets or sets the compatibility version for the plugin.
    /// </summary>
    [JsonPropertyName("targetAbi")]
    public string TargetAbi { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the plugin.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the operational status of this plugin.
    /// </summary>
    [JsonPropertyName("status")]
    public PluginStatus Status { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this plugin should automatically update.
    /// </summary>
    [JsonPropertyName("autoUpdate")]
    public bool AutoUpdate { get; set; } = true; // DO NOT MOVE THIS INTO THE CONSTRUCTOR.

    /// <summary>
    /// Gets or sets the ImagePath
    /// Gets or sets a value indicating whether this plugin has an image.
    /// Image must be located in the local plugin folder.
    /// </summary>
    [JsonPropertyName("imagePath")]
    public string? ImagePath { get; set; }

}