using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TradeBotMarket.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FutureSymbolType
{
    [EnumMember(Value = "BTCUSDT_QUARTER")]
    [Description("Квартальный контракт BTC/USDT")]
    QuarterlyContract,
    
    [EnumMember(Value = "BTCUSDT_BI-QUARTER")]
    [Description("Биквартальный контракт BTC/USDT")]
    BiQuarterlyContract
} 