namespace TradeBotMarket.Domain.Interfaces;

public interface IPriceDifferenceCalculator
{
    Task CalculatePriceDifferencesAsync(DateTime startDate, DateTime endDate);
} 