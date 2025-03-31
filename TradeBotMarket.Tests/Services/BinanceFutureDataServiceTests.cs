using Microsoft.Extensions.Logging;
using Moq;
using TradeBotMarket.DataAccess.Repositories;
using TradeBotMarket.Domain.Constants;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.Domain.Models;
using TradeBotMarket.Domain.Services;
using TradeBotMarket.Domain.Enums;
using Xunit;

namespace TradeBotMarket.Tests.Services;

public class BinanceFutureDataServiceTests
{
    private readonly Mock<ILogger<BinanceFutureDataService>> _loggerMock;
    private readonly Mock<IFuturePriceRepository> _repositoryMock;
    private readonly Mock<IJsonDeserializerService> _jsonDeserializerMock;
    private readonly BinanceFutureDataService _service;

    public BinanceFutureDataServiceTests()
    {
        _loggerMock = new Mock<ILogger<BinanceFutureDataService>>();
        _repositoryMock = new Mock<IFuturePriceRepository>();
        _jsonDeserializerMock = new Mock<IJsonDeserializerService>();
        _service = new BinanceFutureDataService(_loggerMock.Object, _repositoryMock.Object, _jsonDeserializerMock.Object);
    }

    [Fact]
    public async Task CollectAndProcessDataAsync_WhenApiReturnsValidData_ShouldSaveToRepository()
    {
        // Arrange
        var symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue();
        var price = 50000m;
        var now = DateTime.UtcNow;
        var roundedTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(new BinanceExchangeInfo
            {
                Symbols = new List<BinanceSymbolInfo>
                {
                    new() { Symbol = "BTCUSDT_230331", BaseAsset = FutureSymbols.BTC, QuoteAsset = FutureSymbols.USDT, ContractType = FutureSymbols.CURRENT_QUARTER_CONTRACT }
                }
            });

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<BinancePriceResponse>(It.IsAny<string>()))
            .Returns(new BinancePriceResponse { Symbol = "BTCUSDT_230331", Price = price.ToString() });

        // Act
        await _service.CollectAndProcessDataAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.AddPriceAsync(It.Is<FuturePrice>(p =>
                p.Symbol == symbol &&
                p.Price == price &&
                p.IsLastAvailable &&
                !p.IsHistoricalPrice)),
            Times.Once);
    }

    [Fact]
    public async Task CollectAndProcessDataAsync_WhenApiFailsAndHistoricalDataExists_ShouldUseHistoricalPrice()
    {
        // Arrange
        var symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue();
        var historicalPrice = new FuturePrice
        {
            Symbol = symbol,
            Price = 49000m,
            Timestamp = DateTime.UtcNow.AddHours(-1),
            IsLastAvailable = true
        };

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Throws(new Exception("API Error"));

        _repositoryMock
            .Setup(x => x.GetLastAvailablePriceAsync(symbol))
            .ReturnsAsync(historicalPrice);

        // Act
        await _service.CollectAndProcessDataAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.AddPriceAsync(It.Is<FuturePrice>(p =>
                p.Symbol == symbol &&
                p.Price == historicalPrice.Price &&
                !p.IsLastAvailable &&
                p.IsHistoricalPrice)),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoricalPricesAsync_ShouldReturnCorrectPrices()
    {
        // Arrange
        var symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(new BinanceExchangeInfo
            {
                Symbols = new List<BinanceSymbolInfo>
                {
                    new() { Symbol = "BTCUSDT_230331", BaseAsset = FutureSymbols.BTC, QuoteAsset = FutureSymbols.USDT, ContractType = FutureSymbols.CURRENT_QUARTER_CONTRACT }
                }
            });

        var klineData = new List<object[]>
        {
            new object[] { DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), "50000", "51000", "49000", "50500", "100" }
        };

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<List<object[]>>(It.IsAny<string>()))
            .Returns(klineData);

        // Act
        var result = await _service.GetHistoricalPricesAsync(symbol, startDate, endDate, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var price = result.First();
        Assert.Equal(50500m, price.Price);
        Assert.Equal(symbol, price.Symbol);
    }

    [Fact]
    public async Task CollectAndSaveHistoricalDataAsync_ShouldProcessAndSaveData()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue();

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(new BinanceExchangeInfo
            {
                Symbols = new List<BinanceSymbolInfo>
                {
                    new() { Symbol = "BTCUSDT_230331", BaseAsset = FutureSymbols.BTC, QuoteAsset = FutureSymbols.USDT, ContractType = FutureSymbols.CURRENT_QUARTER_CONTRACT }
                }
            });

        var klineData = new List<object[]>
        {
            new object[] { DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), "50000", "51000", "49000", "50500", "100" }
        };

        _jsonDeserializerMock
            .Setup(x => x.Deserialize<List<object[]>>(It.IsAny<string>()))
            .Returns(klineData);

        // Act
        var result = await _service.CollectAndSaveHistoricalDataAsync(startDate, endDate, CancellationToken.None);

        // Assert
        Assert.True(result > 0);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetActualSymbolAsync_WithValidResponse_ReturnsCorrectSymbol()
    {
        // Arrange
        var exchangeInfo = new BinanceExchangeInfo
        {
            Symbols = new List<BinanceSymbol>
            {
                new() { Symbol = "BTCUSDT_230331", BaseAsset = FutureSymbols.BTC, QuoteAsset = FutureSymbols.USDT, ContractType = FutureSymbols.CURRENT_QUARTER_CONTRACT }
            }
        };

        _jsonDeserializerMock.Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(exchangeInfo);

        // Act
        var result = await _service.GetActualSymbolAsync(FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), CancellationToken.None);

        // Assert
        Assert.Equal("BTCUSDT_230331", result);
    }

    [Fact]
    public async Task GetActualSymbolAsync_WithEmptyResponse_ThrowsException()
    {
        // Arrange
        var exchangeInfo = new BinanceExchangeInfo { Symbols = new List<BinanceSymbol>() };
        _jsonDeserializerMock.Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(exchangeInfo);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _service.GetActualSymbolAsync(FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), CancellationToken.None));
    }

    [Fact]
    public async Task GetLatestPriceAsync_WithValidResponse_ReturnsCorrectPrice()
    {
        // Arrange
        var priceResponse = new BinancePriceResponse { Symbol = "BTCUSDT_230331", Price = "50000.00" };
        _jsonDeserializerMock.Setup(x => x.Deserialize<BinancePriceResponse>(It.IsAny<string>()))
            .Returns(priceResponse);

        // Act
        var result = await _service.GetLatestPriceAsync(FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), CancellationToken.None);

        // Assert
        Assert.Equal(50000.00m, result);
    }

    [Fact]
    public async Task GetLatestPriceAsync_WithInvalidResponse_ThrowsException()
    {
        // Arrange
        _jsonDeserializerMock.Setup(x => x.Deserialize<BinancePriceResponse>(It.IsAny<string>()))
            .Returns((BinancePriceResponse)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _service.GetLatestPriceAsync(FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), CancellationToken.None));
    }

    [Fact]
    public async Task CollectAndProcessDataAsync_WithValidData_SavesPrices()
    {
        // Arrange
        var exchangeInfo = new BinanceExchangeInfo
        {
            Symbols = new List<BinanceSymbol>
            {
                new() { Symbol = "BTCUSDT_230331", BaseAsset = FutureSymbols.BTC, QuoteAsset = FutureSymbols.USDT, ContractType = FutureSymbols.CURRENT_QUARTER_CONTRACT }
            }
        };

        var priceResponse = new BinancePriceResponse { Symbol = "BTCUSDT_230331", Price = "50000.00" };

        _jsonDeserializerMock.Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(exchangeInfo);
        _jsonDeserializerMock.Setup(x => x.Deserialize<BinancePriceResponse>(It.IsAny<string>()))
            .Returns(priceResponse);

        // Act
        await _service.CollectAndProcessDataAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.AddPriceAsync(It.IsAny<FuturePrice>()), Times.AtLeastOnce());
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task CollectAndProcessDataAsync_WithApiError_UsesHistoricalPrice()
    {
        // Arrange
        var exchangeInfo = new BinanceExchangeInfo
        {
            Symbols = new List<BinanceSymbol>
            {
                new() { Symbol = "BTCUSDT_230331", BaseAsset = FutureSymbols.BTC, QuoteAsset = FutureSymbols.USDT, ContractType = FutureSymbols.CURRENT_QUARTER_CONTRACT }
            }
        };

        var historicalPrice = new FuturePrice 
        { 
            Symbol = FutureSymbols.QUARTER_CONTRACT_PATTERN, 
            Price = 50000.00m, 
            Timestamp = DateTime.UtcNow.AddHours(-1),
            IsLastAvailable = true
        };

        _jsonDeserializerMock.Setup(x => x.Deserialize<BinanceExchangeInfo>(It.IsAny<string>()))
            .Returns(exchangeInfo);
        _jsonDeserializerMock.Setup(x => x.Deserialize<BinancePriceResponse>(It.IsAny<string>()))
            .Throws(new Exception("API Error"));
        _repositoryMock.Setup(x => x.GetLastAvailablePriceAsync(It.IsAny<string>()))
            .ReturnsAsync(historicalPrice);

        // Act
        await _service.CollectAndProcessDataAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.AddPriceAsync(It.Is<FuturePrice>(p => p.IsHistoricalPrice)), Times.AtLeastOnce());
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce());
    }
} 