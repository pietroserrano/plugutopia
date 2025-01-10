## Engine.TelegramBot

Libreria che contiene l'implementazione dell'Engine per gestire un TelegramBot.

Per poter utilizzare l'Engine di TelegramBot occorre inserire in appsettings.json:

```json
{
  "BotConfiguration": {
    "BotToken": "<bot-token>",
    "Whitelist": [ "user1", "user2" ]
  }
}
```