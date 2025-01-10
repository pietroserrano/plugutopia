namespace Engine.TelegramBot.Data.Entities;

public class ProviderForChat
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; } = default!;
    public long ChatId { get; set; }
}
