using Microsoft.Extensions.Logging;
using Moq;
using TradeBotMarket.Domain.Constants;
using TradeBotMarket.Domain.Models;
using TradeBotMarket.Domain.Services;
using Xunit;

namespace TradeBotMarket.Tests.Services;

public class JsonDeserializerServiceTests
{
    private readonly Mock<ILogger<JsonDeserializerService>> _loggerMock;
    private readonly JsonDeserializerService _service;

    public JsonDeserializerServiceTests()
    {
        _loggerMock = new Mock<ILogger<JsonDeserializerService>>();
        _service = new JsonDeserializerService(_loggerMock.Object);
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsDeserializedObject()
    {
        // Arrange
        var json = $@"{{""symbol"": ""{FutureSymbols.BTCUSDT}"", ""price"": ""50000.00""}}";

        // Act
        var result = _service.Deserialize<BinancePriceResponse>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FutureSymbols.BTCUSDT, result.Symbol);
        Assert.Equal("50000.00", result.Price);
    }

    [Fact]
    public void Deserialize_EmptyJson_ReturnsNull()
    {
        // Arrange
        var json = "";

        // Act
        var result = _service.Deserialize<BinancePriceResponse>(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsException()
    {
        // Arrange
        var json = "invalid json";

        // Act & Assert
        Assert.Throws<JsonException>(() => _service.Deserialize<BinancePriceResponse>(json));
    }

    [Fact]
    public void Deserialize_ArrayData_ReturnsDeserializedList()
    {
        // Arrange
        var json = $@"[
            {{""symbol"": ""{FutureSymbols.BTCUSDT}"", ""price"": ""50000.00""}},
            {{""symbol"": ""{FutureSymbols.BTCUSDT}"", ""price"": ""51000.00""}}
        ]";

        // Act
        var result = _service.Deserialize<List<BinancePriceResponse>>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal(FutureSymbols.BTCUSDT, item.Symbol));
    }

    [Fact]
    public void TryGetDecimalValue_ValidJson_ReturnsTrue()
    {
        // Arrange
        var json = $@"{{""symbol"": ""{FutureSymbols.BTCUSDT}"", ""price"": ""50000.00""}}";

        // Act
        var result = _service.TryGetDecimalValue(json, "price", out decimal value);

        // Assert
        Assert.True(result);
        Assert.Equal(50000.00m, value);
    }

    [Fact]
    public void TryGetDecimalValue_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var result = _service.TryGetDecimalValue(json, "price", out decimal value);

        // Assert
        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetDecimalValue_MissingProperty_ReturnsFalse()
    {
        // Arrange
        var json = $@"{{""symbol"": ""{FutureSymbols.BTCUSDT}""}}";

        // Act
        var result = _service.TryGetDecimalValue(json, "price", out decimal value);

        // Assert
        Assert.False(result);
        Assert.Equal(0, value);
    }
} 