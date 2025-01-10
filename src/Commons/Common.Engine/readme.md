## Common.Engine

Libreria che contiene le classi/interfacce che devono essere implementate per gestire Engine dei vari moduli (come ad esempio TelegramBot)

- IEngine: interfaccia che gli Engine devono implementare per essere caricati dinamicamente
- IEngineRegistrator: interfaccia da implementare per poter registrare le varie dipendenze che l'Engine necessiterà pre e post build della DI
- 