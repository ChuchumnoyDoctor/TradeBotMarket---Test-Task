using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.Domain.Interfaces;

public interface IPriceDifferenceRepository
{
    Task<IEnumerable<PriceDifference>> GetAllDifferencesAsync(int maxItems);
    Task<IEnumerable<PriceDifference>> GetDifferencesBySymbolAsync(int maxItems);
    Task AddDifferenceAsync(PriceDifference difference);
    Task<IEnumerable<PriceDifference>> GetDifferencesAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(PriceDifference difference);
    Task SaveChangesAsync();
    Task<IEnumerable<FuturePrice>> GetPricesForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task SaveDifferencesAsync(IEnumerable<PriceDifference> differences, CancellationToken cancellationToken);
} 