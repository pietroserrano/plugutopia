using Common.Engine.Abstractions;
using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Engine.TelegramBot;

public class TelegramBotEngineRegistrator : IEngineRegistrator
{
    public IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BotConfiguration>(configuration.GetSection(BotConfiguration.SECTION));

        // Register named HttpClient to benefits from IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
                    ArgumentNullException.ThrowIfNull(botConfiguration);
                    TelegramBotClientOptions options = new(botConfiguration.BotToken, botConfiguration.BaseUrl);
                    return new TelegramBotClient(options, httpClient);
                });
        var botConfig = configuration.GetSection(BotConfiguration.SECTION).Get<BotConfiguration>();
        services.AddDbContext<TelegramBotDbContext>(opt => opt.UseSqlite(botConfig!.ConnectionStrings.GetValueOrDefault(BotConfiguration.SQLITE_KEY)));
        services.AddScoped<TelegramBotEngine>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<TelegramBotHostedService>();

        return services;
    }

    public IHost UseApplication(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelegramBotDbContext>();
        //dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();
        try
        {
            var bot = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
            var commands = scope.ServiceProvider.GetInitCommands();

            List<BotCommand> botCommands = [
                new BotCommand() { Command = "/start", Description = "Lista comandi" },
                new BotCommand() { Command = "/end", Description = "Esci da tutte le operazioni" },];
            botCommands.AddRange(commands.Select(command => new BotCommand() { Command = command.Item1.Name, Description = command.Item2 }));

            bot.SetMyCommandsAsync(botCommands, languageCode: "it").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore nel settaggio della lista comandi" + ex.Message);
        }
        return app;
    }
}
