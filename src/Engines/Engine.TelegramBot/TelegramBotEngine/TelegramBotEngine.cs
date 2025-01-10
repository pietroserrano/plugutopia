using Common;
using Common.Engine.Abstractions;
using Common.Extensions;
using Common.Model.Abstractions;
using Common.Model.Abstractions.Engines;
using Common.Model.Models.Types;
using Engine.TelegramBot.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Engine.TelegramBot;

public class TelegramBotEngine(IPluginManager pluginManager, IServiceProvider serviceProvider, ITelegramBotClient bot, TelegramBotDbContext dbContext, IConfiguration configuration, ILogger<IUpdateHandler> logger)
    : IEngine, IUpdateHandler
{
    private readonly IConfiguration _configuration = configuration;
    public Guid Id => new Guid("7548f435-bd0c-4e03-afb2-dbef0a2b3651");
    public string Name => nameof(TelegramBotEngine);
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var username = GetUsername(update);
        if (!IsValidUser(username))
        {
            logger.LogInformation("User '{0}' not in whitelist", username);
            return;
        }

        //TODO: gestire logica tracciamento ChatId e ProviderId
        cancellationToken.ThrowIfCancellationRequested();
        var chat = update.Message?.Chat ?? update.CallbackQuery!.Message!.Chat;
        var entity = await dbContext.ProviderForChats.FirstOrDefaultAsync(p => p.ChatId == chat.Id, cancellationToken);
        var providerId = entity?.ProviderId;
        var printInitCommands = true;
        IPluginModule? plugin = null;
        string customErrorMessage = string.Empty;

        //verifica comando fine conversazione
        if (IsEndConversation(update))
        {
            if (entity is not null)
            {
                var provider = await GetProvider(entity.ProviderId, cancellationToken);
                await provider.EndConversation(update.Message);
                dbContext.ProviderForChats.Remove(entity);
                await dbContext.SaveChangesAsync(cancellationToken);
                customErrorMessage = $"Sessione {entity.ProviderId} terminata!";
            }
            else
            {
                printInitCommands = false;
                customErrorMessage = "Comando errato, invia /start per visualizzare i comandi validi!";
            }

        }
        //verifico sequenza provider già inizializzata
        else if (providerId is not null)
        {
            await GoToProvider(update, providerId.Value, cancellationToken);
            printInitCommands = false;
        }
        //verifico se è un comando di init 
        else if ((plugin = GetPluginIfInitCommand(update)) != null)
        {
            if (dbContext.Users.FirstOrDefault(x => x.ChatId == chat.Id) is null)
                await dbContext.Users.AddAsync(new TelegramUser { Username = username!, ChatId = chat.Id }, cancellationToken);

            await dbContext.ProviderForChats.AddAsync(new ProviderForChat { ChatId = chat.Id, ProviderId = plugin.Id }, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await GoToProvider(update, plugin, cancellationToken);
            printInitCommands = false;
        }

        if (!string.IsNullOrEmpty(customErrorMessage))
            await bot.SendTextMessageAsync(chat, customErrorMessage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);

        if (printInitCommands)
            await PrintInitCommands(chat, cancellationToken);

    }

    private bool IsEndConversation(Update update)
    {
        return update.Message?.Text == "/end";
    }

    private IPluginModule? GetPluginIfInitCommand(Update update)
    {
        if (update is { Message: { } message } || update is { EditedMessage: { } editedMessage })
        {
            //TODO: da verificare quando sarà gestito init command con CallbackQuery
            var cmd = update.Message != null
                ? update.Message.Text!.Split(' ')[0]
                : update.CallbackQuery!.Data!;
            return serviceProvider.GetPluginByInitCommand(new CommandType(cmd));
        }

        return null;
    }

    private async Task GoToProvider(Update update, Guid providerId, CancellationToken cancellationToken)
    {
        var plugin = await GetProvider(providerId, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        await GoToProvider(update, plugin, cancellationToken);
    }

    private Task<IPluginModule> GetProvider(Guid providerId, CancellationToken cancellationToken)
    {
        var plugin = (ITelegramBotEnginePluginModule)serviceProvider.GetPluginById(providerId);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult((IPluginModule)plugin);
    }

    private async Task GoToProvider(Update update, IPluginModule genericIPluginModule, CancellationToken cancellationToken)
    {
        var plugin = (ITelegramBotEnginePluginModule)genericIPluginModule;
        cancellationToken.ThrowIfCancellationRequested();

        await (update switch
        {
            { Message: { } message } => plugin.OnMessage(message),
            { EditedMessage: { } message } => plugin.OnMessage(message),
            { CallbackQuery: { } callbackQuery } => plugin.OnCallbackQuery(callbackQuery),
            { InlineQuery: { } inlineQuery } => plugin.OnInlineQuery(inlineQuery),
            { ChosenInlineResult: { } chosenInlineResult } => plugin.OnChosenInlineResult(chosenInlineResult),
            { Poll: { } poll } => plugin.OnPoll(poll),
            { PollAnswer: { } pollAnswer } => plugin.OnPollAnswer(pollAnswer),
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            _ => plugin.UnknownUpdateHandlerAsync(update)
        });
    }

    private Task PrintInitCommands(Chat chat, CancellationToken cancellationToken)
    {
        var commands = serviceProvider.GetInitCommands();

        string usage = """
                <b>TelegramBotEngine lista plugin</b>:
                <b> </b>
            """;
        foreach (var command in commands)
        {
            usage += $"\n- {command.Item1.Name} : {command.Item2}";
        }
        return bot.SendTextMessageAsync(chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Check if username is in Whitelist
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private bool IsValidUser(string? username)
    {
        var whitelist = _configuration.GetSection("BotConfiguration:Whitelist").Get<string[]>();

        if (string.IsNullOrEmpty(username))
            return false;

        if (whitelist == null || whitelist.Length == 0)
            return true;

        return whitelist.Contains(username);
    }

    private static string? GetUsername(Update update)
        => update switch
        {
            { Message: { } message } => message.From.Username,
            { EditedMessage: { } message } => message.From.Username,
            { CallbackQuery: { } callbackQuery } => callbackQuery.From.Username,
            { InlineQuery: { } inlineQuery } => inlineQuery.From.Username,
            { ChosenInlineResult: { } chosenInlineResult } => chosenInlineResult.From.Username,
            { Poll: { } poll } => string.Empty,
            { PollAnswer: { } pollAnswer } => pollAnswer.User.Username,
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            _ => null
        };
}
