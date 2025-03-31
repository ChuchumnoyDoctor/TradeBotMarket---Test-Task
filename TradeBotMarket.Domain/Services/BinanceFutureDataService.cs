using System.Globalization;
using Microsoft.Extensions.Logging;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.Domain.Models;
using TradeBotMarket.Domain.Constants;
using TradeBotMarket.Domain.Enums;
using TradeBotMarket.Domain.Extensions;

namespace TradeBotMarket.Domain.Services;

public class BinanceFutureDataService : IFutureDataService
{
    private readonly ILogger<BinanceFutureDataService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IJsonDeserializerService _jsonDeserializer;
    private const string BaseUrl = "https://fapi.binance.com/fapi/v1";
    private readonly FutureSymbolType[] _quarterlySymbols = { FutureSymbolType.QuarterlyContract, FutureSymbolType.BiQuarterlyContract };
    private readonly IFuturePriceRepository _repository;

    public BinanceFutureDataService(
        ILogger<BinanceFutureDataService> logger,
        IFuturePriceRepository repository,
        IJsonDeserializerService jsonDeserializer)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _repository = repository;
        _jsonDeserializer = jsonDeserializer;
    }

    private async Task<string> GetActualSymbolAsync(string symbol, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/exchangeInfo", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var exchangeInfo = _jsonDeserializer.Deserialize<BinanceExchangeInfo>(content);

            if (exchangeInfo?.Symbols == null || !exchangeInfo.Symbols.Any())
            {
                throw new Exception("Symbols is null or empty in exchangeInfo response");
            }

            var btcSymbols = exchangeInfo.Symbols
                .Where(s => s.BaseAsset == "BTC" && s.QuoteAsset == "USDT" && !string.IsNullOrEmpty(s.ContractType))
                .ToList();

            var contractTypes = btcSymbols
                .Select(s => new { s.Symbol, s.ContractType })
                .ToList();

            var uniqueContractTypes = contractTypes.Select(c => c.ContractType).Distinct().ToList();

            string desiredContract = symbol == FutureSymbolType.QuarterlyContract.GetEnumMemberValue() 
                ? FutureSymbols.CURRENT_QUARTER_CONTRACT 
                : FutureSymbols.NEXT_QUARTER_CONTRACT;
            
            var actualSymbol = btcSymbols
                .Where(s => s.ContractType.Equals(desiredContract, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Symbol)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(actualSymbol))
            {
                // Если не нашли по точному совпадению, пробуем поискать по частичному совпадению
                actualSymbol = btcSymbols
                    .Where(s => s.ContractType.Contains(FutureSymbols.QUARTER_CONTRACT_PATTERN, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault()?.Symbol;
                
                if (string.IsNullOrEmpty(actualSymbol))
                {
                    throw new Exception($"Failed to find actual symbol for {symbol}. Available contracts: {string.Join(", ", contractTypes.Select(c => $"{c.Symbol} ({c.ContractType})"))}");
                }
                
                _logger.LogWarning("Couldn't find exact contract type match. Using similar contract: {ActualSymbol}", actualSymbol);
            }

            return actualSymbol;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actual symbol for {Symbol}", symbol);
            throw;
        }
    }

    public async Task<decimal> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        try
        {
            var actualSymbol = await GetActualSymbolAsync(symbol, cancellationToken);
            var response = await _httpClient.GetAsync($"{BaseUrl}/ticker/price?symbol={actualSymbol}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var priceResponse = _jsonDeserializer.Deserialize<BinancePriceResponse>(content);

            if (priceResponse == null || !decimal.TryParse(priceResponse.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                _logger.LogError("Failed to parse price response for symbol {Symbol}", actualSymbol);
                
                if (_jsonDeserializer.TryGetDecimalValue(content, "price", out decimal directPrice))
                {
                    return directPrice;
                }
                
                throw new Exception($"Failed to parse price for symbol {actualSymbol}");
            }

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest price for symbol {Symbol}", symbol);
            throw;
        }
    }

    public async Task<IEnumerable<FuturePrice>> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        try
        {
            // Получаем информацию о контракте
            var actualSymbol = await GetActualSymbolAsync(symbol, cancellationToken);
            var startTimeMs = ((DateTimeOffset)startDate).ToUnixTimeMilliseconds();
            var endTimeMs = ((DateTimeOffset)endDate).ToUnixTimeMilliseconds();
            var interval = "1h"; // Используем часовые интервалы вместо минутных для уменьшения объема данных
            
            // Определяем тип контракта
            string contractType = symbol == FutureSymbolType.QuarterlyContract.GetEnumMemberValue() 
                ? FutureSymbols.CURRENT_QUARTER_CONTRACT 
                : FutureSymbols.NEXT_QUARTER_CONTRACT;
            
            _logger.LogInformation("Requesting historical data for {Symbol}, from {StartDate} to {EndDate}", 
                actualSymbol, startDate, endDate);
            
            // Используем continuousKlines для фьючерсных контрактов с базовой парой BTCUSDT
            var requestUrl = $"{BaseUrl}/continuousKlines?pair=BTCUSDT&contractType={contractType}&interval={interval}&startTime={startTimeMs}&endTime={endTimeMs}&limit=1000";
            _logger.LogInformation("Request URL: {Url}", requestUrl);
            
            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(content) || content == "[]")
            {
                _logger.LogWarning("Empty response received for symbol {Symbol}", actualSymbol);
                return Array.Empty<FuturePrice>();
            }
            
            try
            {
                var klines = _jsonDeserializer.Deserialize<List<object[]>>(content);
                
                if (klines == null || !klines.Any())
                {
                    _logger.LogWarning("No klines found for symbol {Symbol}", actualSymbol);
                    return Array.Empty<FuturePrice>();
                }
                
                var prices = new List<FuturePrice>();
                foreach (var kline in klines)
                {
                    if (kline.Length < 5)
                    {
                        _logger.LogWarning("Invalid kline format for {Symbol}: {Kline}", actualSymbol, string.Join(", ", kline));
                        continue;
                    }
                    
                    if (long.TryParse(kline[0].ToString(), out long timestampMs) &&
                        decimal.TryParse(kline[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal closePrice))
                    {
                        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).UtcDateTime;
                        
                        prices.Add(new FuturePrice
                        {
                            Symbol = symbol,
                            Price = closePrice,
                            Timestamp = timestamp,
                            IsLastAvailable = timestamp >= endDate.AddHours(-1)
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Could not parse timestamp or price for kline: {Kline}", string.Join(", ", kline));
                    }
                }
                
                return prices;
            }
            catch (Exception jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing klines for {Symbol}", actualSymbol);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical prices for symbol {Symbol}", symbol);
            throw;
        }
    }

    public async Task CollectAndProcessDataAsync(CancellationToken cancellationToken)
    {
        foreach (var symbol in _quarterlySymbols.Select(x => x.GetEnumMemberValue()))
        {
            try
            {
                var price = await GetLatestPriceAsync(symbol, cancellationToken);

                var now = DateTime.UtcNow;
                var roundedTime = new DateTime(
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour,
                    0,
                    0,
                    DateTimeKind.Utc);

                var existingPrice = await _repository.GetPriceBySymbolAndTimestampAsync(symbol, roundedTime);
                if (existingPrice != null)
                {
                    existingPrice.Price = price;
                    existingPrice.IsLastAvailable = true;
                    await _repository.UpdateAsync(existingPrice);
                }
                else
                {
                    var futurePrice = new FuturePrice
                    {
                        Symbol = symbol,
                        Price = price,
                        Timestamp = roundedTime,
                        IsLastAvailable = true
                    };
                    await _repository.AddPriceAsync(futurePrice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting data for symbol {Symbol}", symbol);
            }
        }

        await _repository.SaveChangesAsync();
    }
    
    /// <summary>
    /// Выгружает и сохраняет исторические данные за указанный период
    /// </summary>
    /// <param name="startDate">Начальная дата периода</param>
    /// <param name="endDate">Конечная дата периода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Количество загруженных записей</returns>
    public async Task<int> CollectAndSaveHistoricalDataAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        int totalRecords = 0;
        
        // Binance API ограничивает выборку до 1000 записей за один запрос
        // Разбиваем период на части, если он слишком большой
        const int maxDaysPerRequest = 7; // Примерно 168 часов (7 дней) по 1-часовому интервалу
        
        for (DateTime currentStart = startDate; currentStart < endDate; currentStart = currentStart.AddDays(maxDaysPerRequest))
        {
            DateTime currentEnd = currentStart.AddDays(maxDaysPerRequest);
            if (currentEnd > endDate)
            {
                currentEnd = endDate;
            }
            
            foreach (var symbol in _quarterlySymbols.Select(x => x.GetEnumMemberValue()))
            {
                try
                {
                    // Получаем исторические данные из API Binance
                    var prices = await GetHistoricalPricesAsync(symbol, currentStart, currentEnd, cancellationToken);
                    
                    if (prices.Any())
                    {
                        // Группируем цены по часам для получения часовых данных
                        var hourlyPrices = prices
                            .GroupBy(p => new DateTime(
                                p.Timestamp.Year,
                                p.Timestamp.Month,
                                p.Timestamp.Day,
                                p.Timestamp.Hour,
                                0,
                                0,
                                DateTimeKind.Utc))
                            .Select(g => new FuturePrice
                            {
                                Symbol = symbol,
                                Price = g.Average(p => p.Price), // Берем среднюю цену за час
                                Timestamp = g.Key,
                                IsLastAvailable = g.Key >= endDate.AddHours(-1) // Если это последний час периода
                            })
                            .OrderBy(p => p.Timestamp)
                            .ToList();
                        
                        // Сохраняем полученные данные в БД
                        foreach (var price in hourlyPrices)
                        {
                            var existingPrice = await _repository.GetPriceBySymbolAndTimestampAsync(
                                symbol, price.Timestamp);
                                
                            if (existingPrice != null)
                            {
                                existingPrice.Price = price.Price;
                                existingPrice.IsLastAvailable = price.IsLastAvailable;
                                await _repository.UpdateAsync(existingPrice);
                            }
                            else
                            {
                                await _repository.AddPriceAsync(price);
                            }
                            
                            totalRecords++;
                        }
                        
                        await _repository.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting historical data for symbol {Symbol} from {Start} to {End}", 
                        symbol, currentStart, currentEnd);
                }
            }
        }
        
        return totalRecords;
    }
}
