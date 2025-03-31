using TradeBotMarket.Domain.Enums;
using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.Domain.Interfaces;

public interface IFuturePriceRepository
{
    Task<IEnumerable<FuturePrice>> GetAllPricesAsync(int maxItems);
    Task<IEnumerable<FuturePrice>> GetPricesBySymbolAsync(FutureSymbolType symbol, int maxItems);
    Task<IEnumerable<FuturePrice>> GetPricesBySymbolAsync(string symbolString, int maxItems);
    Task<FuturePrice?> GetLatestPriceAsync(string symbol);
    Task<IEnumerable<FuturePrice>> GetPricesByDateRangeAsync(string symbol, DateTime startDate, DateTime endDate);
    Task<IEnumerable<FuturePrice>> GetPricesForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task AddPriceAsync(FuturePrice price);
    Task UpdateAsync(FuturePrice price);
    Task<FuturePrice?> GetPriceBySymbolAndTimestampAsync(string symbol, DateTime timestamp);
    Task SaveChangesAsync();
} 