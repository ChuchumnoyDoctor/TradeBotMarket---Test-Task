using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;
using System.Runtime.Serialization;
using TradeBotMarket.Domain.Enums;

namespace TradeBotMarket.ApiService.ModelBinders;

public class FutureSymbolTypeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        // Получаем значение из запроса
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        
        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        // Пытаемся получить значение по EnumMember атрибуту
        if (TryGetEnumValueByEnumMember(value, out FutureSymbolType result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        // Пытаемся конвертировать по имени Enum
        if (Enum.TryParse<FutureSymbolType>(value, true, out var enumValue))
        {
            bindingContext.Result = ModelBindingResult.Success(enumValue);
            return Task.CompletedTask;
        }

        // Если не удалось конвертировать, добавляем ошибку
        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"Значение '{value}' не является допустимым для типа {nameof(FutureSymbolType)}. " +
            $"Допустимые значения: {string.Join(", ", GetAllValidValues())}");

        return Task.CompletedTask;
    }

    private bool TryGetEnumValueByEnumMember(string value, out FutureSymbolType result)
    {
        result = default;
        
        var type = typeof(FutureSymbolType);
        foreach (var field in type.GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
            {
                if (attribute.Value == value)
                {
                    result = (FutureSymbolType)field.GetValue(null);
                    return true;
                }
            }
        }
        
        return false;
    }

    private IEnumerable<string> GetAllValidValues()
    {
        var values = new List<string>();
        
        foreach (var field in typeof(FutureSymbolType).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            // Получаем EnumMember значение
            if (field.GetCustomAttribute<EnumMemberAttribute>() is EnumMemberAttribute attr)
            {
                values.Add(attr.Value);
            }
            
            // Добавляем имя поля Enum
            if (field.Name != "value__")
            {
                values.Add(field.Name);
            }
        }
        
        return values;
    }
} 