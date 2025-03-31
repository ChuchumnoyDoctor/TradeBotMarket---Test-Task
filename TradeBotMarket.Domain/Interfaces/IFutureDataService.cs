using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.Domain.Interfaces;

public interface IFutureDataService
{
    Task<decimal> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken);
    Task<IEnumerable<FuturePrice>> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task CollectAndProcessDataAsync(CancellationToken cancellationToken);
    Task<int> CollectAndSaveHistoricalDataAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
} 