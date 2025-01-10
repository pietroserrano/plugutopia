using Telegram.Bot.Types;

namespace Common.Model.Abstractions.Engines;

public interface ITelegramBotEnginePluginModule : IPluginModule
{
    Task OnCallbackQuery(CallbackQuery callbackQuery);
    Task OnMessage(Message msg);
    Task OnInlineQuery(InlineQuery inlineQuery);
    Task OnChosenInlineResult(ChosenInlineResult chosenInlineResult);
    Task OnPoll(Poll poll);
    Task OnPollAnswer(PollAnswer pollAnswer);
    Task UnknownUpdateHandlerAsync(Update update);
}
