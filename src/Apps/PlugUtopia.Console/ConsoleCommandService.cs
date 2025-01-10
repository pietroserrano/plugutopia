using Common;
using Microsoft.Extensions.Hosting;

namespace PlugUtopia.Console;

public class ConsoleCommandService : BackgroundService
{
    private readonly IPluginManager _pluginManager;

    public ConsoleCommandService(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var exit = false;
        while (!stoppingToken.IsCancellationRequested && !exit)
        {
            System.Console.WriteLine("=== Menu ===");
            System.Console.WriteLine("1. Opzione 1");
            System.Console.WriteLine("0. Esci");
            System.Console.Write("Seleziona un'opzione: ");

            string input = System.Console.ReadLine() ?? throw new InvalidOperationException();
            
            switch (input)
            {
                case "1":
                    await ReloadPlugin();
                    break;
                case "0":
                    exit = true;
                    break;
            }
        }
    }

    private async Task ReloadPlugin()
    {
        //_pluginManager.LoadPlugin();
    }
}