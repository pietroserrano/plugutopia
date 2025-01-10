namespace Engine.TelegramBot.Data.Entities;

public class UserAttribute
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public TelegramUser User { get; set; }
}
