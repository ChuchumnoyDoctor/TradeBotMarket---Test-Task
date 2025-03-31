using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using TradeBotMarket.ApiService.ModelBinders;
using TradeBotMarket.DataAccess.Data;
using TradeBotMarket.DataAccess.Repositories;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.ApiService.Extensions;
using TradeBotMarket.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers(options => 
{
    // Регистрируем кастомный ModelBinder для FutureSymbolType
    options.ModelBinderProviders.Insert(0, new FutureSymbolTypeModelBinderProvider());
})
.AddJsonOptions(options =>
{
    // Используем имена enum вместо числовых значений в JSON
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Настраиваем преобразование enum в route значения
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("FutureSymbolType", typeof(EnumRouteConstraint));
});

// Добавляем поддержку привязки строки к enum в модели
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TradeBotMarket API", Version = "v1" });
    
    // Настройка для отображения enum с описаниями в Swagger
    c.SchemaFilter<EnumSchemaFilter>();
    c.UseInlineDefinitionsForEnums();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IFutureDataService, BinanceFutureDataService>();
builder.Services.AddScoped<IFuturePriceRepository, FuturePriceRepository>();
builder.Services.AddScoped<IPriceDifferenceRepository, PriceDifferenceRepository>();
builder.Services.AddScoped<IJsonDeserializerService, JsonDeserializerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

// Enable CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
