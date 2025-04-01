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
using Serilog;
using Serilog.Events;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Определяем среду выполнения
var isDevelopment = builder.Environment.IsDevelopment();

// Настраиваем Kestrel для прослушивания всех сетевых интерфейсов
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080); // HTTP
    serverOptions.ListenAnyIP(8081, listenOptions => // HTTPS
    {
        listenOptions.UseHttps();
    });
});

// Настройка HTTPS будет осуществляться через переменные окружения ASPNETCORE_URLS и ASPNETCORE_Kestrel__Certificates__Default__Path

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Добавляем HTTP метрики Prometheus
builder.Services.AddMetrics();

// Определяем метрики
var counter = Metrics.CreateCounter("binance_api_requests_total", "Total number of requests to Binance API");
var requestDurationHistogram = Metrics.CreateHistogram("binance_api_request_duration_seconds", "Histogram of Binance API request durations");
var lastPriceGauge = Metrics.CreateGauge("binance_last_price", "Last price from Binance API", new string[] { "symbol" });

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

// Настраиваем сбор метрик Prometheus
app.UseMetricServer();  // Добавляем эндпоинт для метрик
app.UseHttpMetrics();   // Включаем сбор метрик HTTP запросов

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

// Enable CORS
app.UseCors("AllowAll");

// Disabled HTTPS redirection as we're using HTTP only
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Применяем миграции при запуске
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.Run();