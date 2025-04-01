using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using TradeBotMarket.DataAccess.Data;
using TradeBotMarket.DataAccess.Repositories;
using TradeBotMarket.DataCollector.Jobs;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.Domain.Services;
using Prometheus;

// Получаем путь к исполняемому файлу
string basePath = AppContext.BaseDirectory;
string projectPath = Directory.GetCurrentDirectory();

// При запуске из Visual Studio исправляем путь из bin/Debug назад к корню проекта
if (basePath.Contains("bin\\Debug") || basePath.Contains("bin/Debug"))
{
    var directoryInfo = new DirectoryInfo(basePath);
    // Переходим из "bin/Debug/net9.0" в корневую папку проекта
    var projectFolder = directoryInfo.Parent?.Parent?.Parent;
    if (projectFolder != null && projectFolder.Exists)
    {
        projectPath = projectFolder.FullName;
    }
}

// Явно создаем конфигурацию
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(projectPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables();

var configuration = configBuilder.Build();

// В случае, если файл конфигурации не найден, создаем стандартный
if (!File.Exists(Path.Combine(projectPath, "appsettings.json")))
{
    var defaultConfig = @"{
  ""ConnectionStrings"": {
    ""DefaultConnection"": ""Host=localhost;Database=TradeBotMarket;Username=postgres;Password=postgres""
  },
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }
  }
}";
    File.WriteAllText(Path.Combine(projectPath, "appsettings.json"), defaultConfig);
    
    // Перезагружаем конфигурацию
    configBuilder = new ConfigurationBuilder()
        .SetBasePath(projectPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables();
    
    configuration = configBuilder.Build();
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostContext, config) => 
        {
            // Добавляем нашу конфигурацию вместо автоматически созданной
            config.Sources.Clear();
            config.AddConfiguration(configuration);
        })
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            // Регистрируем DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection")));

            // Регистрируем сервисы
            services.AddScoped<IFuturePriceRepository, FuturePriceRepository>();
            services.AddScoped<IPriceDifferenceRepository, PriceDifferenceRepository>();
            services.AddScoped<IJsonDeserializerService, JsonDeserializerService>();
            services.AddScoped<IFutureDataService, BinanceFutureDataService>();
            services.AddScoped<IPriceDifferenceCalculator, PriceDifferenceCalculator>();

            // Настраиваем Quartz.NET
            services.AddQuartz(q =>
            {
                // Регистрируем задачу для сбора текущих цен и расчета разностей за день
                var priceCollectionJobKey = new JobKey("PriceCollectionJob");
                q.AddJob<PriceCollectionJob>(opts => opts.WithIdentity(priceCollectionJobKey));

                // Создаем триггер для планирования запуска каждую минуту
                q.AddTrigger(opts => opts
                    .ForJob(priceCollectionJobKey)
                    .WithIdentity("PriceCollection-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever()));
                        
                // Регистрируем задачу для выгрузки исторических данных за год
                var historicalDataJobKey = new JobKey("HistoricalDataCollectionJob");
                q.AddJob<HistoricalDataCollectionJob>(opts => opts.WithIdentity(historicalDataJobKey));
                
                // Создаем триггер для запуска задачи один раз при старте приложения
                q.AddTrigger(opts => opts
                    .ForJob(historicalDataJobKey)
                    .WithIdentity("HistoricalDataCollection-trigger")
                    .StartNow());
                    
                // Для периодического обновления (например, раз в неделю) можно использовать CronSchedule:
                // .WithCronSchedule("0 0 12 ? * SUN") // Каждое воскресенье в 12:00
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        })
        .Build();

    // Применяем миграции при запуске
    using (var scope = host.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Проверяем, что строка подключения установлена
            if (string.IsNullOrEmpty(context.Database.GetConnectionString()))
            {
                // Если строка не установлена через DI, установим её напрямую
                context.Database.SetConnectionString(configuration.GetConnectionString("DefaultConnection"));
                Log.Information("Строка подключения установлена вручную");
            }
            
            // Проверка соединения с БД перед миграцией
            try
            {
                // Создаем базу данных, если она не существует
                var canConnect = context.Database.CanConnect();
                if (!canConnect)
                {
                    context.Database.EnsureCreated();
                    Log.Information("База данных успешно создана");
                }
                
                context.Database.OpenConnection();
                context.Database.CloseConnection();
            }
            catch (Exception dbEx)
            {
                Log.Error(dbEx, "Не удалось подключиться к PostgreSQL. Проверьте, что сервер запущен и база данных 'TradeBotMarket' создана");
                throw;
            }
            
            context.Database.Migrate();
            Log.Information("Миграции базы данных применены успешно");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при применении миграций базы данных");
            throw;
        }
    }

    // Настраиваем веб-сервер для экспорта метрик Prometheus
    var metricServer = new KestrelMetricServer(80);
    metricServer.Start();
    
    Log.Information("Starting DataCollector Service");
    Log.Information("Prometheus metrics available at http://localhost:80/metrics");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
