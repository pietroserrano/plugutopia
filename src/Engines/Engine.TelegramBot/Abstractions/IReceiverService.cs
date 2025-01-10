namespace Engine.TelegramBot;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken stoppingToken);
}

