namespace Engine.TelegramBot;

public class BotConfiguration
{
    public const string SQLITE_KEY = "Sqlite";
    public const string SECTION = "BotConfiguration";
    public string BotToken { get; init; } = default!;
    public string? BaseUrl { get; init; } = null;
    public Dictionary<string, string> ConnectionStrings { get; set; } = new() { { SQLITE_KEY, "Data Source=telegram_bot.db" } };
}