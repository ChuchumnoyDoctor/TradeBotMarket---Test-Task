namespace TradeBotMarket.Domain.Models;

public class PriceDifference
{
    public int Id { get; set; }
    public string FirstSymbol { get; set; } = null!;
    public string SecondSymbol { get; set; } = null!;
    public decimal FirstPrice { get; set; }
    public decimal SecondPrice { get; set; }
    public decimal Difference { get; set; }
    public DateTime FirstPriceTimestamp { get; set; }
    public DateTime SecondPriceTimestamp { get; set; }
} 