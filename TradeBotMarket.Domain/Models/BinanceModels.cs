using System.Text.Json.Serialization;

namespace TradeBotMarket.Domain.Models;

public class BinancePriceResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;
    
    [JsonPropertyName("time")]
    public long Time { get; set; }
}

public class BinanceExchangeInfo
{
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;
    
    [JsonPropertyName("serverTime")]
    public long ServerTime { get; set; }
    
    [JsonPropertyName("rateLimits")]
    public List<object> RateLimits { get; set; } = new();
    
    [JsonPropertyName("exchangeFilters")]
    public List<object> ExchangeFilters { get; set; } = new();
    
    [JsonPropertyName("assets")]
    public List<object> Assets { get; set; } = new();
    
    [JsonPropertyName("symbols")]
    public List<BinanceSymbolInfo> Symbols { get; set; } = new();
    
    [JsonPropertyName("futuresType")]
    public string FuturesType { get; set; } = string.Empty;
}

public class BinanceSymbolInfo
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("pair")]
    public string Pair { get; set; } = string.Empty;
    
    [JsonPropertyName("contractType")]
    public string ContractType { get; set; } = string.Empty;

    [JsonPropertyName("deliveryDate")]
    public long DeliveryDate { get; set; }
    
    [JsonPropertyName("onboardDate")]
    public long OnboardDate { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("baseAsset")]
    public string BaseAsset { get; set; } = string.Empty;
    
    [JsonPropertyName("quoteAsset")]
    public string QuoteAsset { get; set; } = string.Empty;
    
    [JsonPropertyName("marginAsset")]
    public string MarginAsset { get; set; } = string.Empty;
    
    [JsonPropertyName("pricePrecision")]
    public int PricePrecision { get; set; }
    
    [JsonPropertyName("quantityPrecision")]
    public int QuantityPrecision { get; set; }
    
    [JsonPropertyName("baseAssetPrecision")]
    public int BaseAssetPrecision { get; set; }
    
    [JsonPropertyName("quotePrecision")]
    public int QuotePrecision { get; set; }
    
    [JsonPropertyName("filters")]
    public List<object> Filters { get; set; } = new();
}

public class BinanceKlineResponse
{
    [JsonPropertyName("openTime")]
    public long OpenTime { get; set; }

    [JsonPropertyName("open")]
    public string Open { get; set; } = string.Empty;

    [JsonPropertyName("high")]
    public string High { get; set; } = string.Empty;

    [JsonPropertyName("low")]
    public string Low { get; set; } = string.Empty;

    [JsonPropertyName("close")]
    public string Close { get; set; } = string.Empty;

    [JsonPropertyName("volume")]
    public string Volume { get; set; } = string.Empty;
} 