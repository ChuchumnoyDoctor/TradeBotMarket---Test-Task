using System.Text.Json;

namespace TradeBotMarket.Domain.Interfaces;

/// <summary>
/// Интерфейс для сервиса десериализации JSON
/// </summary>
public interface IJsonDeserializerService
{
    /// <summary>
    /// Десериализует JSON строку в объект указанного типа
    /// </summary>
    T? Deserialize<T>(string json) where T : class;
    
    /// <summary>
    /// Извлекает значение из JSON по указанному пути и парсит его в decimal
    /// </summary>
    bool TryGetDecimalValue(string json, string propertyName, out decimal value);
} 