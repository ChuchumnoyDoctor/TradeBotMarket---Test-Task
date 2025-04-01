# TradeBotMarket

Проект для анализа разницы цен между квартальными фьючерсными контрактами BTCUSDT на бирже Binance.

## Архитектура

Проект построен на основе микросервисной архитектуры и состоит из следующих компонентов:

### Сервисы
- **ApiService** - REST API для получения данных о ценах и разнице между контрактами
- **DataCollector** - Консольное приложение для сбора и обработки данных с Binance API, работающее по расписанию

### Библиотеки
- **Domain** - Бизнес-логика, интерфейсы и модели
- **DataAccess** - Работа с базой данных, репозитории и миграции
- **Tests** - Unit и интеграционные тесты

## Технологии

### Backend
- **.NET 9.0** - Основной фреймворк
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM для работы с базой данных
- **Quartz.NET** - Планировщик задач
- **Prometheus-net** - Сбор метрик
- **Serilog** - Логирование

### База данных
- **PostgreSQL** - Основная база данных

### Мониторинг
- **Prometheus** - Сбор и хранение метрик
- **Grafana** - Визуализация метрик

### Контейнеризация
- **Docker** - Контейнеризация сервисов
- **Docker Compose** - Оркестрация контейнеров

### Тестирование
- **xUnit** - Фреймворк для тестирования
- **Moq** - Библиотека для мокирования
- **WebApplicationFactory** - Интеграционное тестирование

## Функциональность

1. Сбор данных:
   - Получение цен квартальных фьючерсов BTCUSDT
   - Вычисление разницы между ценами
   - Сохранение исторических данных
   - Обработка ситуаций с отсутствующими данными

2. API Endpoints:
   - GET /api/Futures - получение всех цен с пагинацией (maxItems)
   - GET /api/Futures/{symbol} - получение цен по символу (QuarterlyContract/BiQuarterlyContract)
   - GET /api/Futures/period - получение цен за период (startDate, endDate)
   - GET /api/Futures/price-differences - получение разницы цен с пагинацией
   - GET /api/Futures/price-differences/{symbol} - получение разницы цен по символу

3. Мониторинг:
   - Разница цен между контрактами
   - Количество запросов к Binance API
   - Время ответа API
   - Количество ошибок
   - Количество записей в базе данных

## Конфигурация и безопасность

### HTTPS и сертификаты
1. Генерация сертификатов:
   ```powershell
   # Запустите скрипт из директории certs
   .\generate-cert.ps1
   ```

2. Конфигурация портов:
   - HTTP: 8080
   - HTTPS: 8081

3. Переменные окружения для HTTPS:
   ```env
   ASPNETCORE_URLS=http://+:8080;https://+:8081
   ASPNETCORE_Kestrel__Certificates__Default__Path=/path/to/certificate.pfx
   ASPNETCORE_Kestrel__Certificates__Default__Password=your_password
   ```

## Запуск проекта

1. Клонировать репозиторий:
```bash
git clone https://github.com/yourusername/TradeBotMarket.git
```

2. Сгенерировать SSL сертификаты:
```powershell
cd certs
.\generate-cert.ps1
```

3. Запустить через Docker Compose:
```bash
docker-compose up -d
```

4. Доступные сервисы:
   - API HTTP: http://localhost:8080
   - API HTTPS: https://localhost:8081
   - Swagger: http://localhost:8080/swagger или https://localhost:8081/swagger
   - Grafana: http://localhost:3000 (admin/admin)
     - Дашборд с метриками разницы цен
     - Мониторинг производительности API
     - Статистика сбора данных
   - Prometheus: http://localhost:9090
     - Сбор метрик с API и DataCollector
     - Хранение исторических данных метрик

## Разработка

1. Требования:
   - .NET SDK 9.0
   - Docker Desktop
   - PostgreSQL (если запуск без Docker)

2. Настройка базы данных:
```bash
dotnet ef database update --project TradeBotMarket.DataAccess
```

3. Запуск тестов:
```bash
dotnet test
```

## Архитектурные решения

1. Clean Architecture:
   - Разделение на слои (Domain, DataAccess, Services)
   - Инверсия зависимостей
   - Единая точка ответственности

2. Паттерны:
   - Repository Pattern
   - Unit of Work
   - Dependency Injection
   - Observer (для метрик)

3. Обработка ошибок:
   - Глобальная обработка исключений через Middleware
   - Логирование через Serilog с ротацией файлов
   - Метрики ошибок в Prometheus

4. Масштабируемость:
   - Микросервисная архитектура
   - Контейнеризация
   - Независимое масштабирование сервисов

## Метрики и мониторинг

1. Бизнес-метрики:
   - Разница цен между контрактами
   - Количество обработанных контрактов
   - Использование исторических цен

2. Технические метрики:
   - Latency API запросов
   - Количество ошибок
   - Размер базы данных
   - Время выполнения задач

## Логирование

1. Конфигурация Serilog:
   - Логирование в консоль
   - Ротация файлов логов по дням
   - Путь к логам: `logs/api-.txt`

2. Уровни логирования:
   - Debug - для детальной отладки
   - Information - для основных событий
   - Warning - для предупреждений
   - Error - для ошибок

## Безопасность

1. CORS:
   - Настроена политика AllowAll для разработки
   - Возможность настройки под конкретные домены

2. HTTPS:
   - Поддержка SSL/TLS
   - Автоматическая генерация сертификатов
   - Конфигурируемые порты
