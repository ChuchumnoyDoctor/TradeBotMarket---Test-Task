# TradeBotMarket

Система для сбора и анализа данных о ценах фьючерсных контрактов Bitcoin с биржи Binance.

## Назначение

TradeBotMarket предназначен для:
- Сбора актуальных цен фьючерсных контрактов BTC/USDT (квартальных и биквартальных)
- Выгрузки исторических данных за выбранные периоды
- Расчета разницы цен между различными типами контрактов
- Хранения исторических данных и расчетов в базе данных
- Предоставления API для доступа к собранным данным

## Архитектура проекта

Проект разделен на несколько модулей:

### TradeBotMarket.Domain
Содержит основные модели данных, интерфейсы и константы:
- Модели данных:
  - `FuturePrice` - модель для хранения исторических цен фьючерсов
  - `PriceDifference` - модель для хранения разницы цен
- Интерфейсы:
  - `IFutureDataService` - сервис для получения данных о ценах с биржи Binance
  - `IFuturePriceRepository` - репозиторий для работы с ценами фьючерсов
  - `IPriceDifferenceRepository` - репозиторий для работы с разницей цен
  - `IJsonDeserializerService` - сервис для десериализации JSON
- Enums:
  - `FutureSymbolType` - типы фьючерсных контрактов (QuarterlyContract, BiQuarterlyContract)
- Константы:
  - `FutureSymbols` - константы для работы с фьючерсными контрактами

### TradeBotMarket.DataAccess
Содержит компоненты для работы с базой данных:
- `ApplicationDbContext` - контекст Entity Framework Core
- Репозитории:
  - `FuturePriceRepository` - реализация репозитория для цен фьючерсов
  - `PriceDifferenceRepository` - реализация репозитория для разницы цен
- Миграции базы данных для PostgreSQL

### TradeBotMarket.DataCollector
Сервис для сбора и обработки данных:
- `BinanceFutureDataService` - реализация сервиса для работы с API Binance
- `JsonDeserializerService` - сервис для десериализации JSON
- `PriceDifferenceCalculator` - сервис для расчета разницы цен
- Задачи Quartz.NET:
  - `PriceCollectionJob` - задача для сбора текущих цен
  - `HistoricalDataCollectionJob` - задача для выгрузки исторических данных

### TradeBotMarket.ApiService
API для доступа к данным:
- `FuturesController` - контроллер для доступа к ценам и разницам цен
- ModelBinders:
  - `FutureSymbolTypeModelBinder` - кастомный биндер для преобразования строк в FutureSymbolType
- Middleware:
  - `ExceptionHandlingMiddleware` - обработка исключений в API
- Swagger для документации API
- Настройка CORS для разрешения кросс-доменных запросов

## Функциональность

- **Сбор текущих цен**: Каждую минуту система собирает актуальные цены фьючерсных контрактов
- **Исторические данные**: При старте выгружаются исторические данные за последний год
- **Расчет разницы цен**: Система рассчитывает разницу между ценами различных типов контрактов
- **API для доступа к данным**: REST API с ограничением количества возвращаемых элементов

## Требования для запуска

- .NET 9.0
- PostgreSQL
- Quartz.NET
- Доступ к API Binance

## Настройка базы данных

1. Установите PostgreSQL
2. Создайте базу данных с именем `TradeBotMarket`
3. Настройте строку подключения в `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TradeBotMarket;Username=postgres;Password=postgres"
  }
}
```

## Миграция базы данных

Миграция базы данных выполняется автоматически при запуске приложения. Если вы хотите выполнить миграцию вручную, используйте следующие команды:

1. Установите инструменты Entity Framework Core:
```bash
dotnet tool install --global dotnet-ef
```

2. Выполните миграцию:
```bash
cd TradeBotMarket.DataAccess
dotnet ef database update
```

3. Для создания новой миграции (при изменении моделей):
```bash
cd TradeBotMarket.DataAccess
dotnet ef migrations add MigrationName
```

## Запуск проекта

### Запуск DataCollector (сбор данных)
```bash
cd TradeBotMarket.DataCollector
dotnet run
```

### Запуск API
```bash
cd TradeBotMarket.ApiService
dotnet run
```

API будет доступно по адресу `http://localhost:5000` и `https://localhost:7001`.
Документация Swagger: `http://localhost:5000/swagger`

## Основные эндпоинты API

- `GET /api/Futures` - Получение всех цен (с ограничением maxItems, по умолчанию 100)
- `GET /api/Futures/{symbol}` - Получение цен по типу контракта (Enum: QuarterlyContract, BiQuarterlyContract)
- `GET /api/Futures/period?startDate={date}&endDate={date}` - Получение цен за период
- `GET /api/Futures/price-differences` - Получение всех разниц цен (с ограничением maxItems)
- `GET /api/Futures/price-differences/{symbol}` - Получение разниц цен по типу контракта (Enum)

## Особенности работы с API Binance

Для получения данных по фьючерсным контрактам используется API Binance Futures:
- Для получения текущих цен: `/fapi/v1/ticker/price`
- Для получения исторических данных: `/fapi/v1/continuousKlines?pair=BTCUSDT&contractType={contractType}`
- Для получения информации о доступных контрактах: `/fapi/v1/exchangeInfo`

## Особенности реализации

- **Enum для типов контрактов**: Использование enum `FutureSymbolType` с поддержкой атрибутов EnumMember
- **Кастомный ModelBinder**: Реализован для корректного преобразования строковых значений в enum
- **Улучшенная документация Swagger**: Отображение enum с описаниями и возможными значениями
- **Ограничение количества элементов**: Все запросы API с получением списков имеют параметр `maxItems`
- **Сортировка данных**: Данные из БД возвращаются отсортированными по времени (от новых к старым)
- **Обработка ошибок**: Middleware для обработки исключений и возврата структурированных ошибок
- **CORS**: Настроена политика CORS для доступа к API из браузерных приложений

## Исходный код

Код организован с использованием лучших практик .NET:
- SOLID принципы проектирования
- Dependency Injection для управления зависимостями
- Repository Pattern для работы с данными
- Clean Architecture для разделения ответственности между слоями 