using TradeBotMarket.Domain.Models;
using TradeBotMarket.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TradeBotMarket.Domain.Enums;
using TradeBotMarket.Domain.Extensions;

namespace TradeBotMarket.Domain.Services;

public class PriceDifferenceCalculator : IPriceDifferenceCalculator
{
    private readonly IPriceDifferenceRepository _repository;
    private readonly ILogger<PriceDifferenceCalculator> _logger;

    public PriceDifferenceCalculator(
        IPriceDifferenceRepository repository,
        ILogger<PriceDifferenceCalculator> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task CalculatePriceDifferencesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var prices = await _repository.GetPricesForPeriodAsync(startDate, endDate, CancellationToken.None);
            if (!prices.Any())
            {
                _logger.LogWarning("No prices found for the specified period");
                return;
            }

            // Группируем цены по часам
            var groupedPrices = prices
                .GroupBy(p => new DateTime(
                    p.Timestamp.Year,
                    p.Timestamp.Month,
                    p.Timestamp.Day,
                    p.Timestamp.Hour,
                    0,
                    0,
                    DateTimeKind.Utc));

            var differences = new List<PriceDifference>();

            foreach (var group in groupedPrices)
            {
                var quarterPrice = group
                    .Where(p => p.Symbol == FutureSymbolType.QuarterlyContract.GetEnumMemberValue())
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                var biQuarterPrice = group
                    .Where(p => p.Symbol == FutureSymbolType.BiQuarterlyContract.GetEnumMemberValue())
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                if (quarterPrice != null && biQuarterPrice != null)
                {
                    var difference = new PriceDifference
                    {
                        FirstSymbol = quarterPrice.Symbol,
                        SecondSymbol = biQuarterPrice.Symbol,
                        FirstPrice = quarterPrice.Price,
                        SecondPrice = biQuarterPrice.Price,
                        Difference = biQuarterPrice.Price - quarterPrice.Price,
                        FirstPriceTimestamp = quarterPrice.Timestamp,
                        SecondPriceTimestamp = biQuarterPrice.Timestamp
                    };

                    differences.Add(difference);
                }
            }

            if (differences.Any())
            {
                await _repository.SaveDifferencesAsync(differences, CancellationToken.None);
                _logger.LogInformation("Calculated {Count} price differences", differences.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price differences");
            throw;
        }
    }
} 