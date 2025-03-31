using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TradeBotMarket.Domain.Interfaces;

namespace TradeBotMarket.Domain.Services;

/// <summary>
/// Реализация сервиса десериализации JSON
/// </summary>
public class JsonDeserializerService : IJsonDeserializerService
{
    private readonly ILogger<JsonDeserializerService> _logger;
    private readonly JsonSerializerOptions _options;

    public JsonDeserializerService(ILogger<JsonDeserializerService> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string json) where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("Empty JSON string received");
                return default;
            }

            try
            {
                if (typeof(T) == typeof(List<object[]>))
                {
                    using var document = JsonDocument.Parse(json);
                    var root = document.RootElement;
                    if (!root.EnumerateArray().Any())
                    {
                        return default;
                    }

                    var result = new List<object[]>();
                    foreach (var item in root.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        var array = item.EnumerateArray()
                            .Select(x => x.ValueKind switch
                            {
                                JsonValueKind.String => x.GetString(),
                                JsonValueKind.Number => x.GetDecimal(),
                                _ => (object?)x.ToString()
                            })
                            .ToArray();

                        result.Add(array);
                    }

                    return (T)(object)result;
                }

                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing JSON");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing JSON to type {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public bool TryGetDecimalValue(string json, string propertyName, out decimal value)
    {
        value = 0;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty(propertyName, out JsonElement element))
            {
                var valueStr = element.GetString();
                if (valueStr != null && decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }

            _logger.LogWarning("Could not extract decimal value for property {Property} from JSON", propertyName);
            return false;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing JSON when extracting {Property}", propertyName);
            return false;
        }
    }
}
