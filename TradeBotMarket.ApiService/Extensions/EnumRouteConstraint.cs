using System.Runtime.Serialization;
using TradeBotMarket.Domain.Enums;

namespace TradeBotMarket.ApiService.Extensions;

public class EnumRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext httpContext, IRouter route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var value) || value == null)
            return false;

        var stringValue = value.ToString();
        
        // Проверяем, можно ли конвертировать значение в FutureSymbolType
        try
        {
            if (TryParseWithEnumMember<FutureSymbolType>(stringValue, out _))
                return true;
                
            // Стандартный парсинг по имени enum
            if (Enum.TryParse<FutureSymbolType>(stringValue, true, out _))
                return true;
                
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    private bool TryParseWithEnumMember<T>(string value, out T result) where T : struct, Enum
    {
        result = default;
        
        var type = typeof(T);
        foreach (var field in type.GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
            {
                if (attribute.Value == value)
                {
                    result = (T)field.GetValue(null);
                    return true;
                }
            }
        }
        
        return false;
    }
} 