#FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Apps/PlugUtopia.Console/PlugUtopia.Console.csproj", "Apps/PlugUtopia.Console/"]
COPY ["Engines/Engine.TelegramBot/Engine.TelegramBot.csproj", "Engines/Engine.TelegramBot/"]
COPY ["Commons/Common.Engine/Common.Engine.csproj", "Commons/Common.Engine/"]
COPY ["Commons/Common/Common.csproj", "Commons/Common/"]
COPY ["Commons/Common.Plugin/Common.Plugin.csproj", "Commons/Common.Plugin/"]
RUN dotnet nuget add source "https://pkgs.dev.azure.com/tgbots/Telegram.Bot/_packaging/release/nuget/v3/index.json" -n Telegram.Bot
RUN dotnet restore "Apps/PlugUtopia.Console/PlugUtopia.Console.csproj"
COPY . .
WORKDIR "/src/Apps/PlugUtopia.Console"
RUN dotnet build "PlugUtopia.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PlugUtopia.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN if [ -d "appsettings.*.json" ]; then rm -Rf appsettings.*.json; fi

RUN mkdir db
RUN chown -R app:app /app
USER app
ENTRYPOINT ["dotnet", "PlugUtopia.Console.dll"]
