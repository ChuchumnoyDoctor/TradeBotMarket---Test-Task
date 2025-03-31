using Microsoft.AspNetCore.Mvc.ModelBinding;
using TradeBotMarket.Domain.Enums;

namespace TradeBotMarket.ApiService.ModelBinders;

public class FutureSymbolTypeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.Metadata.ModelType == typeof(FutureSymbolType))
            return new FutureSymbolTypeModelBinder();

        return null;
    }
} 