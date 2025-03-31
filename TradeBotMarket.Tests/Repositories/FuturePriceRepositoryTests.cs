using Microsoft.EntityFrameworkCore;
using TradeBotMarket.DataAccess.Data;
using TradeBotMarket.DataAccess.Repositories;
using TradeBotMarket.Domain.Enums;
using TradeBotMarket.Domain.Models;
using Xunit;

namespace TradeBotMarket.Tests.Repositories;

public class FuturePriceRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FuturePriceRepository _repository;

    public FuturePriceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new FuturePriceRepository(_context);
    }

    [Fact]
    public async Task GetAllPricesAsync_ShouldReturnOrderedPrices()
    {
        // Arrange
        var prices = new List<FuturePrice>
        {
            new() { Symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), Price = 50000m, Timestamp = DateTime.UtcNow.AddHours(-2) },
            new() { Symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), Price = 51000m, Timestamp = DateTime.UtcNow.AddHours(-1) },
            new() { Symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), Price = 52000m, Timestamp = DateTime.UtcNow }
        };

        await _context.FuturePrices.AddRangeAsync(prices);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllPricesAsync(10);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal(52000m, result.First().Price);
        Assert.Equal(50000m, result.Last().Price);
    }

    [Fact]
    public async Task GetPricesBySymbolAsync_ShouldReturnOnlyMatchingSymbol()
    {
        // Arrange
        var prices = new List<FuturePrice>
        {
            new() { Symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), Price = 50000m, Timestamp = DateTime.UtcNow },
            new() { Symbol = FutureSymbolType.BiQuarterlyContract.GetEnumMemberValue(), Price = 51000m, Timestamp = DateTime.UtcNow }
        };

        await _context.FuturePrices.AddRangeAsync(prices);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPricesBySymbolAsync(FutureSymbolType.QuarterlyContract, 10);

        // Assert
        Assert.Single(result);
        Assert.Equal(FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), result.First().Symbol);
    }

    [Fact]
    public async Task GetLastAvailablePriceAsync_ShouldReturnMostRecentPrice()
    {
        // Arrange
        var symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue();
        var prices = new List<FuturePrice>
        {
            new() { Symbol = symbol, Price = 50000m, Timestamp = DateTime.UtcNow.AddHours(-2), IsLastAvailable = true },
            new() { Symbol = symbol, Price = 51000m, Timestamp = DateTime.UtcNow.AddHours(-1), IsLastAvailable = true },
            new() { Symbol = symbol, Price = 52000m, Timestamp = DateTime.UtcNow, IsLastAvailable = false }
        };

        await _context.FuturePrices.AddRangeAsync(prices);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLastAvailablePriceAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(51000m, result.Price);
    }

    [Fact]
    public async Task GetPricesByDateRangeAsync_ShouldReturnPricesInRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(-2);
        var endDate = now;
        var symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue();

        var prices = new List<FuturePrice>
        {
            new() { Symbol = symbol, Price = 50000m, Timestamp = now.AddHours(-3) },
            new() { Symbol = symbol, Price = 51000m, Timestamp = now.AddHours(-2) },
            new() { Symbol = symbol, Price = 52000m, Timestamp = now.AddHours(-1) },
            new() { Symbol = symbol, Price = 53000m, Timestamp = now }
        };

        await _context.FuturePrices.AddRangeAsync(prices);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPricesByDateRangeAsync(symbol, startDate, endDate);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, price => 
            Assert.True(price.Timestamp >= startDate && price.Timestamp <= endDate));
    }

    [Fact]
    public async Task GetTotalRecordsCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var prices = new List<FuturePrice>
        {
            new() { Symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), Price = 50000m, Timestamp = DateTime.UtcNow },
            new() { Symbol = FutureSymbolType.BiQuarterlyContract.GetEnumMemberValue(), Price = 51000m, Timestamp = DateTime.UtcNow }
        };

        await _context.FuturePrices.AddRangeAsync(prices);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalRecordsCountAsync();

        // Assert
        Assert.Equal(2, result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 