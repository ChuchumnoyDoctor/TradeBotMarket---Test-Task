using System.Text.Json;
using TradeBotMarket.Domain.Exceptions;

namespace TradeBotMarket.ApiService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API Error: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server Error: {Message}", ex.Message);
            await HandleExceptionAsync(context, new ApiException("Internal Server Error"));
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, ApiException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception.StatusCode;

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = exception.StatusCode,
            Message = exception.Message
        });

        await context.Response.WriteAsync(result);
    }
} 