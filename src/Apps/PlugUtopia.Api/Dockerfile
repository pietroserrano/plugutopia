FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Apps/PlugUtopia.Api/PlugUtopia.Api.csproj", "Apps/PlugUtopia.Api/"]
RUN dotnet restore "Apps/PlugUtopia.Api/PlugUtopia.Api.csproj"
COPY . .
WORKDIR "/src/Apps/PlugUtopia.Api"
RUN dotnet build "PlugUtopia.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PlugUtopia.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlugUtopia.Api.dll"]
