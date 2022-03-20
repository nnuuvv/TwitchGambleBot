FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TwitchGambleBot/TwitchGambleBot.csproj", "TwitchGambleBot/"]
RUN dotnet restore "TwitchGambleBot/TwitchGambleBot.csproj"
COPY . .
WORKDIR "/src/TwitchGambleBot"
RUN dotnet build "TwitchGambleBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TwitchGambleBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TwitchGambleBot.dll"]
