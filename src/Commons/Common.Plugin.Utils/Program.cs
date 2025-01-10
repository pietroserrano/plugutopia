if (args.Length > 2 && args[0] == "--generate-manifest")
{
    string outputPath = Path.Combine(args[1], "manifest.json");
    string assemblyName = $"{args[2]}.dll";
    File.WriteAllText(outputPath, PluginManifestHelper.GeneratePluginManifest(assemblyName));
    Console.WriteLine($"Manifest scritto in: {outputPath}");
    return;
}
else if (args.Length > 1 && args[0] == "--generate-package")
{
    var destinationPath = args.Length > 2 ? args[2] : null;
    PluginPackageHelpers.GeneratePackage(args[1], destinationPath);
    return;
}