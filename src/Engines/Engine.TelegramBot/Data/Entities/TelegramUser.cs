namespace Engine.TelegramBot.Data.Entities;

public class TelegramUser
{
    public long ChatId { get; set; }
    public string Username { get; set; } = null!;
    public ICollection<UserAttribute> Attributes { get; set; }
}
