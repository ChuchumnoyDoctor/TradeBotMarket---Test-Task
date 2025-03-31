using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace TradeBotMarket.Domain.Extensions;

public static class EnumExtensions
{
    public static string GetEnumMemberValue<T>(this T value) where T : Enum
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
            
        var enumType = typeof(T);
        var memberInfo = enumType.GetMember(value.ToString());
        var enumMemberAttribute = memberInfo[0].GetCustomAttribute<EnumMemberAttribute>();
        
        return enumMemberAttribute?.Value ?? value.ToString();
    }
    
    public static string GetDescription<T>(this T value) where T : Enum
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
            
        var enumType = typeof(T);
        var memberInfo = enumType.GetMember(value.ToString());
        var descriptionAttribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
        
        return descriptionAttribute?.Description ?? value.ToString();
    }
    
    public static T GetEnumFromValue<T>(string value) where T : Enum
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));
            
        var enumType = typeof(T);
        
        foreach (var field in enumType.GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
            {
                if (attribute.Value == value)
                    return (T)field.GetValue(null);
            }
        }
        
        // В случае если не найдено соответствие EnumMember, попробуем использовать имя
        return (T)Enum.Parse(enumType, value, true);
    }
} 