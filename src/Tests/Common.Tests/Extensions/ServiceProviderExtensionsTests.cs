namespace Common.Tests.Extensions;

public class ServiceProviderExtensionsTests
{
    internal class TestPluginModule : IPluginModule
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<CommandType> Commands { get; set; }

        public TestPluginModule()
        {
            Id = Guid.NewGuid();
            Commands = new List<CommandType>() { new CommandType("initCommand") };
            Name = "PluginTest";
        }

        public TestPluginModule(List<CommandType> commands) : base()
        {
            ArgumentNullException.ThrowIfNull(commands, nameof(commands));
            Commands?.AddRange(commands);
        }

        public bool IsInitConversationCommand(CommandType commandType)
            => Commands.Any(c => c.Name.Equals(commandType.Name));

        public CommandType GetInitConversationCommandType()
             => Commands.First(c => c.Name == "initCommand");

        public List<CommandType> GetCommandTypes()
             => Commands;
    }

    [Fact]
    public void GetPluginByName_Successfully()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        var moduleFromProvider = provider.GetPluginByName(module.Name);

        Assert.NotNull(moduleFromProvider);
        Assert.Equal(module.Name, moduleFromProvider.Name);
        Assert.Equal(module.Id, moduleFromProvider.Id);
        Assert.Equal(module.Commands, moduleFromProvider.GetCommandTypes());
    }

    [Fact]
    public void GetPluginByName_Throw_Exception()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetPluginByName("LoremIpsum"));
        Assert.Throws<ArgumentNullException>(() => provider.GetPluginByName(null));
        provider = null;
        Assert.Throws<ArgumentNullException>(() => provider.GetPluginByName("LoremIpsum"));
    }

    [Fact]
    public void GetPluginById_Successfully()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        var moduleFromProvider = provider.GetPluginById(module.Id);

        Assert.NotNull(moduleFromProvider);
        Assert.Equal(module.Name, moduleFromProvider.Name);
        Assert.Equal(module.Id, moduleFromProvider.Id);
        Assert.Equal(module.Commands, moduleFromProvider.GetCommandTypes());
    }

    [Fact]
    public void GetPluginById_Throw_Exception()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetPluginById(Guid.NewGuid()));
        provider = null;
        Assert.Throws<ArgumentNullException>(() => provider.GetPluginById(Guid.NewGuid()));
    }

    [Fact]
    public void GetPluginByInitCommand_Successfully()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        var moduleFromProvider = provider.GetPluginByInitCommand(module.GetInitConversationCommandType());

        Assert.NotNull(moduleFromProvider);
        Assert.Equal(module.Name, moduleFromProvider.Name);
        Assert.Equal(module.Id, moduleFromProvider.Id);
        Assert.Equal(module.Commands, moduleFromProvider.GetCommandTypes());
    }

    [Fact]
    public void GetPluginByInitCommand_Throw_Exception()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetPluginByInitCommand(new CommandType("LoremIpsum")));

        Assert.Throws<ArgumentNullException>(() => provider.GetPluginByInitCommand(null));
        provider = null;
        Assert.Throws<ArgumentNullException>(() => provider.GetPluginByInitCommand(new CommandType("LoremIpsum")));
    }

    [Fact]
    public void GetInitCommands_Successfully()
    {
        var services = new ServiceCollection();
        var module = new TestPluginModule();
        services.AddSingleton<IPluginModule>(module);

        var provider = services.BuildServiceProvider();

        var commands = provider.GetInitCommands();

        Assert.NotNull(commands);
        Assert.NotEmpty(commands);
    }

    [Fact]
    public void GetInitCommands_Throw_Exception()
    {
        var services = new ServiceCollection();

        var provider = services.BuildServiceProvider();

        var commands = provider.GetInitCommands();

        Assert.NotNull(commands);
        Assert.Empty(commands);

        provider = null;
        Assert.Throws<ArgumentNullException>(() => provider.GetPluginByInitCommand(new CommandType("LoremIpsum")));
    }

}
