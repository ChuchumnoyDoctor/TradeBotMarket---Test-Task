FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TradeBotMarket.DataCollector/TradeBotMarket.DataCollector.csproj", "TradeBotMarket.DataCollector/"]
COPY ["TradeBotMarket.Domain/TradeBotMarket.Domain.csproj", "TradeBotMarket.Domain/"]
COPY ["TradeBotMarket.DataAccess/TradeBotMarket.DataAccess.csproj", "TradeBotMarket.DataAccess/"]
RUN dotnet restore "TradeBotMarket.DataCollector/TradeBotMarket.DataCollector.csproj"
COPY TradeBotMarket.DataCollector/. TradeBotMarket.DataCollector/
COPY TradeBotMarket.Domain/. TradeBotMarket.Domain/
COPY TradeBotMarket.DataAccess/. TradeBotMarket.DataAccess/
WORKDIR "/src/TradeBotMarket.DataCollector"
RUN dotnet publish "TradeBotMarket.DataCollector.csproj" -c Release -o /app/publish/collector

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish/collector .
ENTRYPOINT ["dotnet", "TradeBotMarket.DataCollector.dll"]