using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using TradeBotMarket.ApiService;
using TradeBotMarket.DataAccess.Data;
using TradeBotMarket.Domain.Constants;
using TradeBotMarket.Domain.Models;
using Xunit;

namespace TradeBotMarket.Tests.Integration;

public class FuturesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FuturesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();

                // Добавляем тестовые данные
                db.FuturePrices.AddRange(new[]
                {
                    new FuturePrice
                    {
                        Symbol = FutureSymbolType.QuarterlyContract.GetEnumMemberValue(),
                        Price = 50000m,
                        Timestamp = DateTime.UtcNow,
                        IsLastAvailable = true
                    },
                    new FuturePrice
                    {
                        Symbol = FutureSymbolType.BiQuarterlyContract.GetEnumMemberValue(),
                        Price = 51000m,
                        Timestamp = DateTime.UtcNow,
                        IsLastAvailable = true
                    }
                });

                db.SaveChanges();
            });
        });
    }

    [Fact]
    public async Task GetAllPrices_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Futures?maxItems=10");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetPricesBySymbol_ReturnsMatchingPrices()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Futures/QuarterlyContract?maxItems=10");
        var content = await response.Content.ReadAsStringAsync();
        var prices = JsonSerializer.Deserialize<IEnumerable<FuturePrice>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(prices);
        Assert.All(prices, price => Assert.Equal(FutureSymbolType.QuarterlyContract.GetEnumMemberValue(), price.Symbol));
    }

    [Fact]
    public async Task GetPricesByDateRange_ReturnsCorrectPrices()
    {
        // Arrange
        var client = _factory.CreateClient();
        var startDate = DateTime.UtcNow.AddDays(-1).ToString("O");
        var endDate = DateTime.UtcNow.ToString("O");

        // Act
        var response = await client.GetAsync($"/api/Futures/period?startDate={startDate}&endDate={endDate}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var prices = JsonSerializer.Deserialize<IEnumerable<FuturePrice>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(prices);
        Assert.All(prices, price =>
            Assert.True(price.Timestamp >= DateTime.UtcNow.AddDays(-1) && price.Timestamp <= DateTime.UtcNow));
    }

    [Fact]
    public async Task GetPricesByInvalidSymbol_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Futures/InvalidSymbol");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 