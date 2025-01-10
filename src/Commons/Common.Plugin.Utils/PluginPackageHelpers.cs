using Common.Model.Models;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

public static class PluginPackageHelpers
{
    public static void GeneratePackage(string pluginManifest, string? destinationZipFile)
    {
        if (string.IsNullOrWhiteSpace(pluginManifest) || !File.Exists(pluginManifest))
            throw new FileNotFoundException(nameof(pluginManifest));
        pluginManifest = Path.GetFullPath(pluginManifest);

        using var stream = File.OpenRead(pluginManifest);
        var manifest = JsonSerializer.Deserialize<PluginManifest>(stream)
            ?? throw new Exception("manifest corrupted");

        string pluginPath = Path.GetDirectoryName(pluginManifest)!;
        if (string.IsNullOrWhiteSpace(destinationZipFile))
            destinationZipFile = Path.Combine(pluginPath, "..");

        destinationZipFile = Path.GetFullPath(destinationZipFile);
        if (!Directory.Exists(destinationZipFile))
            throw new DirectoryNotFoundException(nameof(destinationZipFile));

        string zipName = $"{manifest.Name}_{manifest.Version}.zip";
        string md5Name = $"{zipName}.md5.txt";
        string zipPath = Path.Combine(destinationZipFile, zipName);
        string md5Path = Path.Combine(destinationZipFile, md5Name);

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(pluginPath, zipPath);

        string md5Hash = GetMD5HashOfZipFile(zipPath);
        Console.WriteLine($"File saved into : {zipPath}");
        Console.WriteLine($"MD5 Hash: {md5Hash}");
        File.WriteAllText(md5Path, md5Hash);
    }

    public static string GetMD5HashOfZipFile(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        byte[] hashBytes = md5.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
