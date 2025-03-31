namespace TradeBotMarket.Domain.Models;

public class FuturePrice
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsLastAvailable { get; set; }
} 