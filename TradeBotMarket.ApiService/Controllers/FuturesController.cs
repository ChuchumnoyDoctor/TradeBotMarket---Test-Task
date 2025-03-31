using Microsoft.AspNetCore.Mvc;
using TradeBotMarket.Domain.Enums;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuturesController : ControllerBase
{
    private readonly IFuturePriceRepository _futurePriceRepository;
    private readonly IPriceDifferenceRepository _priceDifferenceRepository;
    private const int DefaultMaxItems = 100;

    public FuturesController(
        IFuturePriceRepository futurePriceRepository,
        IPriceDifferenceRepository priceDifferenceRepository)
    {
        _futurePriceRepository = futurePriceRepository;
        _priceDifferenceRepository = priceDifferenceRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FuturePrice>>> GetAllPrices([FromQuery] int? maxItems = DefaultMaxItems)
    {
        var prices = await _futurePriceRepository.GetAllPricesAsync(maxItems ?? DefaultMaxItems);
        return Ok(prices);
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<IEnumerable<FuturePrice>>> GetPricesBySymbol(FutureSymbolType symbol, [FromQuery] int? maxItems = DefaultMaxItems)
    {
        var prices = await _futurePriceRepository.GetPricesBySymbolAsync(symbol, maxItems ?? DefaultMaxItems);
        return Ok(prices);
    }

    [HttpGet("period")]
    public async Task<ActionResult<IEnumerable<FuturePrice>>> GetPricesForPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var prices = await _futurePriceRepository.GetPricesForPeriodAsync(startDate, endDate, HttpContext.RequestAborted);
        return Ok(prices);
    }

    [HttpGet("price-differences")]
    public async Task<ActionResult<IEnumerable<PriceDifference>>> GetAllPriceDifferences([FromQuery] int? maxItems = DefaultMaxItems)
    {
        var differences = await _priceDifferenceRepository.GetAllDifferencesAsync(maxItems ?? DefaultMaxItems);
        return Ok(differences);
    }

    [HttpGet("price-differences/{symbol}")]
    public async Task<ActionResult<IEnumerable<PriceDifference>>> GetPriceDifferencesBySymbol([FromQuery] int? maxItems = DefaultMaxItems)
    {
        var differences = await _priceDifferenceRepository.GetDifferencesBySymbolAsync(maxItems ?? DefaultMaxItems);
        return Ok(differences);
    }
} 