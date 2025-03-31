using Microsoft.Extensions.Logging;
using Quartz;
using TradeBotMarket.Domain.Interfaces;

namespace TradeBotMarket.DataCollector.Jobs;

/// <summary>
/// Задача по выгрузке исторических данных из Binance и расчету разницы цен
/// </summary>
[DisallowConcurrentExecution]
public class HistoricalDataCollectionJob : IJob
{
    private readonly IFutureDataService _futureDataService;
    private readonly IPriceDifferenceCalculator _calculator;
    private readonly ILogger<HistoricalDataCollectionJob> _logger;

    public HistoricalDataCollectionJob(
        IFutureDataService futureDataService,
        IPriceDifferenceCalculator calculator,
        ILogger<HistoricalDataCollectionJob> logger)
    {
        _futureDataService = futureDataService;
        _calculator = calculator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting historical data collection job");

        try
        {
            // Выгружаем данные за последний год
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddYears(-1);
            
            _logger.LogInformation("Collecting historical data from {StartDate} to {EndDate}", 
                startDate, endDate);
            
            // Выгружаем и сохраняем исторические данные из Binance
            int recordsCount = await _futureDataService.CollectAndSaveHistoricalDataAsync(
                startDate, endDate, context.CancellationToken);
            
            _logger.LogInformation("Successfully collected {RecordsCount} historical price records", 
                recordsCount);
            
            // Рассчитываем разницы цен на основе загруженных данных
            await _calculator.CalculatePriceDifferencesAsync(startDate, endDate);
            
            _logger.LogInformation("Historical data collection and processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing historical data collection job");
        }
    }
} 