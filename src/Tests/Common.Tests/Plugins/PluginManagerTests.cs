namespace Common.Tests.Plugins;

public class PluginManagerTests
{
    private static readonly string _testPathRoot = Path.Combine(Path.GetTempPath(), "pmp-test-data");
    private string _pluginFolder = string.Empty;
    private string _tempPath = string.Empty;

    public PluginManagerTests()
    {
        (_tempPath, _pluginFolder) = GetTestPaths("plugin-" + Path.GetRandomFileName());
        Directory.CreateDirectory(_pluginFolder);
    }

    [Theory]
    [InlineData("test-plugin/some.dll", 3)]
    [InlineData("test-plugin2/some2.dll", 0)]
    public async Task DiscoverPlugin_Status_Enabled(string dllFile, int dependencyAssemblyCount)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version("1.0.0");
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            ReferencedAssemblies = dependencyAssemblyCount != 0 ? Enumerable.Range(1, dependencyAssemblyCount).Select(i => $"{i}_{dllFile}").ToList() : new(),
            TargetAbi = version.ToString()
        };

        
        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile) ,PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);

        var res = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath));

        //var expectedFullPath = Path.Combine(_pluginFolder, dllFile).Canonicalize();

        Assert.NotNull(res);
        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.ENABLED, pluginLoaded.Manifest.Status);
        Assert.Equal(dependencyAssemblyCount, pluginLoaded.Manifest.ReferencedAssemblies.Count);
    }

    [Theory]
    [InlineData("test-plugin/some.dll")]
    public async Task GetAppVersionFromPluginAssembly(string dllFile)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version("1.0.0");
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = version.ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, "");

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration);

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.MALFUNCTIONED, pluginLoaded.Manifest.Status);
    }

    [Theory]
    [InlineData("test-plugin/some.dll")]
    public async Task PluginWithoutManifest_Malfunctioned(string dllFile)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version("1.0.0");
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = version.ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        File.Create(Path.Combine(dllPath, filename));
        //var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        //File.WriteAllText(manifestPath, "");

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration);

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.MALFUNCTIONED, pluginLoaded.Manifest.Status);

        pluginManager.Dispose();
    }

    [Theory]
    [InlineData("test-plugin/some.dll")]
    public async Task EmptyPLugins_IfPluginDirectoryNotExists(string dllFile)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version("1.0.0");
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = version.ToString()
        };

        if(Directory.Exists(_pluginFolder))
            Directory.Delete(_pluginFolder);

        //var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        //Directory.CreateDirectory(dllPath);
        //File.Create(Path.Combine(dllPath, filename));
        //var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        //File.WriteAllText(manifestPath, "");

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration);

        Assert.Empty(pluginManager.GetPlugins());
    }

    [Theory]
    [InlineData("test-plugin/some.dll")]
    public async Task DiscoverPlugin_Status_Malfunctioned_ManifestNotValid(string dllFile)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version("1.0.0");
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = version.ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, "");

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.MALFUNCTIONED, pluginLoaded.Manifest.Status);
    }

    [Theory]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task DiscoverAndLoadPlugin_State_Enabled(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;
        
        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.ENABLED, pluginLoaded.Manifest.Status);
        Assert.NotNull(pluginLoaded.Assembly);
        Assert.Equal(manifest.AssemblyName, pluginLoaded.Assembly.GetName().Name);
        Assert.NotNull(pluginLoaded.Registrator);
        Assert.False(pluginLoaded.IsInError);
        Assert.True(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));

        Assert.NotEmpty(pluginManager.GetPluginAssemblies());
        Assert.Equal(pluginLoaded.Assembly, pluginManager.GetPluginAssemblies().First());

        pluginManager.Dispose();
    }

    [Theory]
    [InlineData("test-plugin-malfunctioned/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin-malfunctioned/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin-malfunctioned/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin-malfunctioned/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin-malfunctioned/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin-malfunctioned/some.dll", "1.1.0", "1.1.1")]
    public async Task DiscoverAndLoadPlugin_State_Malfunctioned(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath, true);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.MALFUNCTIONED, pluginLoaded.Manifest.Status);
        Assert.NotNull(pluginLoaded.Assembly);
        Assert.Equal(manifest.AssemblyName, pluginLoaded.Assembly.GetName().Name);
        Assert.Null(pluginLoaded.Registrator);
        Assert.True(pluginLoaded.IsInError);
        Assert.False(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));
    }

    [Theory]
    [InlineData("test-plugin-not-supported/some.dll", "1.1.0")]
    [InlineData("test-plugin-not-supported/some.dll", "1.1.1")]
    [InlineData("test-plugin-not-supported/some.dll", "2.1.0")]
    [InlineData("test-plugin-not-supported/some.dll", "2.1.1")]
    public async Task DiscoverAndLoadPlugin_State_NotSupported(string dllFile, string appVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version("1.0.0").ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        Assert.Equal(PluginStatus.NOT_SUPPORTED, pluginLoaded.Manifest.Status);

        Assert.Null(pluginLoaded.Assembly);
        Assert.Equal(manifest.Name, pluginLoaded.Name);
        Assert.Null(pluginLoaded.Registrator);
        Assert.True(pluginLoaded.IsInError);
        Assert.False(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));
    }

    [Theory]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task LoadPluginAndServices_State_Enabled(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDllWithRegistrator(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        pluginManager.RegisterServices(new ServiceCollection());
        pluginManager.UseApplications(null);

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = (LocalPlugin)pluginManager.GetPlugins().First();


        Assert.Equal(PluginStatus.ENABLED, pluginLoaded.Manifest.Status);
        Assert.NotNull(pluginLoaded.Assembly);
        Assert.Equal(manifest.AssemblyName, pluginLoaded.Assembly.GetName().Name);
        Assert.NotNull(pluginLoaded.Registrator);
        Assert.False(pluginLoaded.IsInError);
        Assert.True(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));

        Assert.NotEmpty(pluginManager.GetPluginAssemblies());
        Assert.Equal(pluginLoaded.Assembly, pluginManager.GetPluginAssemblies().First());

        Assert.True((bool)pluginLoaded.Registrator.GetType().GetProperty("AddServiceIsInvoked").GetValue(pluginLoaded.Registrator));
        Assert.True((bool)pluginLoaded.Registrator.GetType().GetProperty("UseApplicationIsInvoked").GetValue(pluginLoaded.Registrator));
    }

    //fail, enable, disable e remove
    [Theory]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task DisablePlugin_Successfully(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        pluginManager.DisablePlugin(pluginLoaded);

        var res = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath));

        Assert.Equal(PluginStatus.RESTART, pluginLoaded.Manifest.Status);
        Assert.Equal(PluginStatus.DISABLED, res.Status);

        Assert.NotNull(pluginLoaded.Assembly);
        Assert.Equal(manifest.AssemblyName, pluginLoaded.Assembly.GetName().Name);
        Assert.NotNull(pluginLoaded.Registrator);
        Assert.False(pluginLoaded.IsInError);
        Assert.True(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));

        Assert.NotEmpty(pluginManager.GetPluginAssemblies());
        Assert.Equal(pluginLoaded.Assembly, pluginManager.GetPluginAssemblies().First());
    }

    [Theory]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task EnablePlugin_Successfully(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        pluginManager.DisablePlugin(pluginLoaded);

        var res = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath));

        Assert.Equal(PluginStatus.RESTART, pluginLoaded.Manifest.Status);
        Assert.Equal(PluginStatus.DISABLED, res.Status);

        pluginManager.EnablePlugin(pluginLoaded);

        res = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath));

        Assert.Equal(PluginStatus.ENABLED, pluginLoaded.Manifest.Status);
        Assert.Equal(PluginStatus.ENABLED, res.Status);

        Assert.NotNull(pluginLoaded.Assembly);
        Assert.Equal(manifest.AssemblyName, pluginLoaded.Assembly.GetName().Name);
        Assert.NotNull(pluginLoaded.Registrator);
        Assert.False(pluginLoaded.IsInError);
        Assert.True(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));

        Assert.NotEmpty(pluginManager.GetPluginAssemblies());
        Assert.Equal(pluginLoaded.Assembly, pluginManager.GetPluginAssemblies().First());
    }

    [Theory]
    [InlineData("test-plugin-1/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task RemovePlugin_Successfully(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString()
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());
        var pluginLoaded = pluginManager.GetPlugins().First();

        pluginManager.RemovePlugin(pluginLoaded);

        Assert.Equal(PluginStatus.RESTART, pluginLoaded.Manifest.Status);

        if (File.Exists(manifestPath))
        {
            var res = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath));
            Assert.Equal(PluginStatus.DELETED, res.Status);
        }
    }

    [Theory]
    [InlineData("test-plugin-1/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task RemovePlugin_AtStartup_Successfully(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString(),
            Status = PluginStatus.DELETED
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDll(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.Empty(pluginManager.GetPlugins());
    }

    [Theory]
    [InlineData("test-plugin-1/some.dll", "1.0.0", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.1", "1.0.0")]
    [InlineData("test-plugin/some.dll", "1.0.0", "1.0.1")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.1", "1.1.0")]
    [InlineData("test-plugin/some.dll", "1.1.0", "1.1.1")]
    public async Task DisabledPlugin_AtStartup_NotLoaded_Successfully(string dllFile, string appVersion, string pluginVersion)
    {
        Dictionary<string, string> inMemoryConf = new()
        {
            { $"{PluginConfiguration.SECTION}:{nameof(PluginConfiguration.Directory)}", _pluginFolder }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConf!)
            .Build();

        //Creare plugin farlocchi
        var filename = Path.GetFileName(dllFile)!;

        var version = new Version(appVersion);
        var manifest = new PluginManifest
        {
            Id = Guid.NewGuid(),
            Name = "Test Assembly",
            AssemblyName = Path.GetFileNameWithoutExtension(filename),
            TargetAbi = new Version(pluginVersion).ToString(),
            Status = PluginStatus.DISABLED
        };


        var dllPath = Path.GetDirectoryName(Path.Combine(_pluginFolder, dllFile))!;

        Directory.CreateDirectory(dllPath);
        CreatePluginDllWithRegistrator(Path.GetFileNameWithoutExtension(filename), dllPath);

        //File.Create(Path.Combine(dllPath, filename));
        var manifestPath = Path.Combine(_pluginFolder, Path.GetDirectoryName(dllFile), PluginManifest.FileName);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        var pluginManager = new PluginManager(new NullLogger<IPluginManager>(), configuration, version);
        pluginManager.LoadPlugin();

        Assert.NotEmpty(pluginManager.GetPlugins());

        var pluginLoaded = (LocalPlugin)pluginManager.GetPlugins().First();


        Assert.Equal(PluginStatus.DISABLED, pluginLoaded.Manifest.Status);
        Assert.NotNull(pluginLoaded.Assembly);
        Assert.Equal(manifest.AssemblyName, pluginLoaded.Assembly.GetName().Name);
        Assert.NotNull(pluginLoaded.Registrator);
        Assert.False(pluginLoaded.IsInError);
        Assert.True(string.IsNullOrEmpty(pluginLoaded.ErrorMessage));

        Assert.NotEmpty(pluginManager.GetPluginAssemblies());
        Assert.Equal(pluginLoaded.Assembly, pluginManager.GetPluginAssemblies().First());

        Assert.False((bool)pluginLoaded.Registrator.GetType().GetProperty("AddServiceIsInvoked").GetValue(pluginLoaded.Registrator));
        Assert.False((bool)pluginLoaded.Registrator.GetType().GetProperty("UseApplicationIsInvoked").GetValue(pluginLoaded.Registrator));
    }

    private (string TempPath, string PluginFolder) GetTestPaths(string pluginFolderName)
    {
        var tempPath = Path.Combine(_testPathRoot, "plugin-manager" + Path.GetRandomFileName());
        var pluginFolder = Path.Combine(tempPath, pluginFolderName);

        return (tempPath, pluginFolder);
    }

    private void CreatePluginDll(string assemblyName, string? directory = null, bool withoutRegistrator = false)
    {
        string dllFileName = $"{assemblyName}.dll";

        // Definizione dell'assembly
        AssemblyName aName = new AssemblyName(assemblyName);
        PersistedAssemblyBuilder assemblyBuilder = new PersistedAssemblyBuilder(aName, typeof(object).Assembly);

        // Definizione del modulo
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
            assemblyName);

        Type interfaceType = typeof(IPluginRegistrator);
        // Creazione della classe che implementa l'interfaccia
        TypeBuilder classBuilder = moduleBuilder.DefineType(
            "MyPluginRegistrator",
            TypeAttributes.Public,
            null,
            withoutRegistrator ? new Type[] { } : new Type[] { interfaceType }); // Implementa l'interfaccia

        if (!withoutRegistrator)
        {
            // Creazione del tipo "MyPlugin"
            TypeBuilder myPluginTypeBuilder = moduleBuilder.DefineType(
                "MyPlugin",
                TypeAttributes.Public | TypeAttributes.Class);

            // Aggiunta di un costruttore senza parametri
            ConstructorBuilder ctorBuilder = myPluginTypeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            ILGenerator ctorIL = ctorBuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Ret);

            // Completa il tipo "MyPlugin"
            Type myPluginType = myPluginTypeBuilder.CreateType();


            MethodInfo interfaceMethod = interfaceType.GetMethod("AddServices");
            MethodInfo interfaceMethod2 = interfaceType.GetMethod("UseApplication");

            // Implementazione del metodo Greet
            MethodBuilder methodBuilder = classBuilder.DefineMethod(
                interfaceMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                interfaceMethod.ReturnType,
                Array.ConvertAll(interfaceMethod.GetParameters(), p => p.ParameterType));

            // Implementazione del metodo Greet
            MethodBuilder methodBuilder2 = classBuilder.DefineMethod(
                interfaceMethod2.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                interfaceMethod2.ReturnType,
                Array.ConvertAll(interfaceMethod2.GetParameters(), p => p.ParameterType));

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ret);

            ilGenerator = methodBuilder2.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ret);


            classBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
            classBuilder.DefineMethodOverride(methodBuilder2, interfaceMethod2);
        }

        // Completa la definizione della classe
        Type dynamicType = classBuilder.CreateType();

        // Salva la DLL su file
        assemblyBuilder.Save(Path.Combine(directory ?? string.Empty, dllFileName));
    }

    private void CreatePluginDllWithRegistrator(string assemblyName, string? directory = null)
    {
        static MethodBuilder AddPropertyWithBackingField(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            // Crea il campo privato per la proprietà
            FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{propertyName.ToLower()}", propertyType, FieldAttributes.Private);

            // Crea la proprietà
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Crea il getter
            MethodBuilder getterMethodBuilder = typeBuilder.DefineMethod(
                $"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                propertyType,
                Type.EmptyTypes);

            ILGenerator getterIL = getterMethodBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            // Crea il setter
            MethodBuilder setterMethodBuilder = typeBuilder.DefineMethod(
                $"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                null,
                new[] { propertyType });

            ILGenerator setterIL = setterMethodBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            // Associa il getter e il setter alla proprietà
            propertyBuilder.SetGetMethod(getterMethodBuilder);
            propertyBuilder.SetSetMethod(setterMethodBuilder);

            // Ritorna il metodo del setter per l'uso successivo
            return setterMethodBuilder;
        }

        static void AddAddServicesMethod(TypeBuilder typeBuilder, MethodBuilder addServiceIsInvokedSetMethod)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "AddServices",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(IServiceCollection),
                new[] { typeof(IServiceCollection), typeof(IConfiguration) });

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            // Corpo del metodo:
            // AddServiceIsInvoked = true;
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldc_I4_1);
            ilGenerator.Emit(OpCodes.Call, addServiceIsInvokedSetMethod);

            // return services;
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ret);

            // Implementa l'interfaccia
            typeBuilder.DefineMethodOverride(methodBuilder, typeof(IPluginRegistrator).GetMethod("AddServices"));
        }

        static void AddUseApplicationMethod(TypeBuilder typeBuilder, MethodBuilder useApplicationIsInvokedSetMethod)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "UseApplication",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(void),
                new[] { typeof(IServiceProvider) });

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            // Corpo del metodo:
            // UseApplicationIsInvoked = true;
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldc_I4_1);
            ilGenerator.Emit(OpCodes.Call, useApplicationIsInvokedSetMethod);

            ilGenerator.Emit(OpCodes.Ret);

            // Implementa l'interfaccia
            typeBuilder.DefineMethodOverride(methodBuilder, typeof(IPluginRegistrator).GetMethod("UseApplication"));
        }

        string dllFileName = $"{assemblyName}.dll";

        // Creazione dell'assembly
        AssemblyName aName = new AssemblyName(assemblyName);
        PersistedAssemblyBuilder assemblyBuilder = new PersistedAssemblyBuilder(aName, typeof(object).Assembly);

        // Definizione del modulo
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
            assemblyName);


        // Creazione della classe dinamica
        TypeBuilder classBuilder = moduleBuilder.DefineType(
            "MyPluginRegistratorForDI",
            TypeAttributes.Public | TypeAttributes.Class);

        // Implementa l'interfaccia IPluginRegistrator
        classBuilder.AddInterfaceImplementation(typeof(IPluginRegistrator));

        // Proprietà: AddServiceIsInvoked
        var addServiceIsInvokedSetMethod = AddPropertyWithBackingField(classBuilder, "AddServiceIsInvoked", typeof(bool));

        // Proprietà: UseApplicationIsInvoked
        var useApplicationIsInvokedSetMethod = AddPropertyWithBackingField(classBuilder, "UseApplicationIsInvoked", typeof(bool));

        // Metodo: AddServices
        AddAddServicesMethod(classBuilder, addServiceIsInvokedSetMethod);

        // Metodo: UseApplication
        AddUseApplicationMethod(classBuilder, useApplicationIsInvokedSetMethod);

        // Completa la definizione della classe
        Type myPluginRegistratorType = classBuilder.CreateType();

        // Salva l'assembly come DLL
        assemblyBuilder.Save(Path.Combine(directory ?? string.Empty, dllFileName));
    }
}
