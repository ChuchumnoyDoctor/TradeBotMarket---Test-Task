using Microsoft.Extensions.Logging;
using Quartz;
using TradeBotMarket.Domain.Interfaces;

namespace TradeBotMarket.DataCollector.Jobs;

public class PriceCollectionJob : IJob
{
    private readonly IFutureDataService _futureDataService;
    private readonly IPriceDifferenceCalculator _calculator;
    private readonly ILogger<PriceCollectionJob> _logger;

    public PriceCollectionJob(
        IFutureDataService futureDataService,
        IPriceDifferenceCalculator calculator,
        ILogger<PriceCollectionJob> logger)
    {
        _futureDataService = futureDataService;
        _calculator = calculator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting price collection job");

        try
        {
            // Собираем текущие цены
            await _futureDataService.CollectAndProcessDataAsync(context.CancellationToken);

            // Рассчитываем разницы цен за последний день
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-1);
            await _calculator.CalculatePriceDifferencesAsync(startDate, endDate);
            
            _logger.LogInformation("Price collection job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing price collection job");
        }
    }
} 