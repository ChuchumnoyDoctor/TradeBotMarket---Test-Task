FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TradeBotMarket.ApiService/TradeBotMarket.ApiService.csproj", "TradeBotMarket.ApiService/"]
COPY ["TradeBotMarket.Domain/TradeBotMarket.Domain.csproj", "TradeBotMarket.Domain/"]
COPY ["TradeBotMarket.DataAccess/TradeBotMarket.DataAccess.csproj", "TradeBotMarket.DataAccess/"]
RUN dotnet restore "TradeBotMarket.ApiService/TradeBotMarket.ApiService.csproj"
COPY TradeBotMarket.ApiService/. TradeBotMarket.ApiService/
COPY TradeBotMarket.Domain/. TradeBotMarket.Domain/
COPY TradeBotMarket.DataAccess/. TradeBotMarket.DataAccess/
WORKDIR "/src/TradeBotMarket.ApiService"
RUN dotnet publish "TradeBotMarket.ApiService.csproj" -c Release -o /app/publish/api

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish/api .
ENTRYPOINT ["dotnet", "TradeBotMarket.ApiService.dll"]