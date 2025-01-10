using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Engine.TelegramBot;

public class TelegramBotHostedService(IServiceProvider serviceProvider, ILogger<TelegramBotHostedService> logger)
    : PollingServiceBase<ReceiverService>(serviceProvider, logger);

public class ReceiverService(ITelegramBotClient botClient, TelegramBotEngine updateHandler, ILogger<ReceiverServiceBase<TelegramBotEngine>> logger)
    : ReceiverServiceBase<TelegramBotEngine>(botClient, updateHandler, logger);