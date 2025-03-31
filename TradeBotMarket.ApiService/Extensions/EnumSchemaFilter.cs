using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace TradeBotMarket.ApiService.Extensions;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = new List<IOpenApiAny>();
            var enumDescriptions = new Dictionary<string, string>();
            
            foreach (var fieldInfo in context.Type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                // Получаем значение EnumMember для отображения в Swagger
                var enumMemberAttribute = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();
                var enumValue = enumMemberAttribute?.Value ?? fieldInfo.Name;
                
                // Добавляем значение в список Enum
                enumNames.Add(new OpenApiString(enumValue));
                
                // Получаем описание для документации
                var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    enumDescriptions[enumValue] = descriptionAttribute.Description;
                }
            }
            
            schema.Enum = enumNames;
            
            // Если есть описания, добавляем их в схему
            if (enumDescriptions.Count > 0)
            {
                schema.Description = $"{schema.Description ?? "Возможные значения:"}\n\n";
                foreach (var kvp in enumDescriptions)
                {
                    schema.Description += $"- {kvp.Key}: {kvp.Value}\n";
                }
            }
        }
    }
} 