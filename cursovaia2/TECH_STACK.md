# 🛠️ Технологический стек и зависимости

## 📦 Основные технологии

### Backend
- **Runtime**: .NET 10
- **Framework**: ASP.NET Core MVC
- **Language**: C# 13.0

### Database
- **ORM**: Entity Framework Core 9.0.0
- **Database**: PostgreSQL 15+
- **Driver**: Npgsql.EntityFrameworkCore.PostgreSQL 9.0.1

### Frontend
- **UI Framework**: Bootstrap 5.3.0
- **Client-side**: JavaScript (Fetch API)
- **View Engine**: Razor Pages/Views

### Tools
- **.NET CLI**: 10.0.0
- **Package Manager**: NuGet
- **Entity Framework Tool**: dotnet-ef 10.0.8

---

## 📋 NuGet пакеты (Dependencies)

### Core Packages
```
Microsoft.EntityFrameworkCore                  9.0.0
Microsoft.EntityFrameworkCore.Design           9.0.0
Microsoft.EntityFrameworkCore.SqlServer        9.0.0
Npgsql.EntityFrameworkCore.PostgreSQL          9.0.1
Microsoft.AspNetCore.App                       10.0.0
```

### Authentication & Security
```
System.Security.Cryptography                   (встроенно в .NET 10)
Microsoft.AspNetCore.Identity                  (встроенно)
System.IdentityModel.Tokens.Jwt               (встроенно)
```

### Additional
```
Microsoft.AspNetCore.Session                   (встроенно)
Microsoft.AspNetCore.StaticFiles               (встроенно)
```

---

## 🏗️ Архитектура приложения

### Слои

#### 1. **Presentation Layer** (Views & Controllers)
- Controllers: 6 контроллеров
- Views: Razor templates (HTML + C#)
- Static Files: CSS, JavaScript

#### 2. **Application Layer** (Models & Services)
- Models: C# классы для данных
- Services: Бизнес-логика
- SessionExtensions: Расширения для работы с сессией

#### 3. **Data Access Layer** (EF Core)
- ApplicationDbContext: DbContext для работы с БД
- ModelsDb: Классы-сущности для EF Core
- Migrations: EF Core миграции

#### 4. **Database Layer** (PostgreSQL)
- 18 таблиц
- Отношения между таблицами
- Индексы и constraints

---

## 💾 База данных - Таблицы и типы

### Пользователи и роли
```sql
roles                    -- Роли пользователей
users                    -- Пользователи
customers                -- Информация о покупателях
```

### Товары и категории
```sql
categories               -- Категории товаров
brands                   -- Бренды
products                 -- Товары
product_images           -- Изображения товаров
product_attributes       -- Характеристики товаров
```

### Корзина
```sql
carts                    -- Корзины пользователей
cart_items               -- Товары в корзине
```

### Заказы
```sql
orders                   -- Заказы
order_items              -- Товары в заказах
payments                 -- Платежи
payment_methods          -- Методы оплаты
deliveries               -- Доставка заказов
delivery_methods         -- Методы доставки
```

### Дополнительно
```sql
discounts                -- Скидки
promo_codes              -- Промокоды
reviews                  -- Отзывы
wishlists                -- Вишлисты
```

---

## 🔐 Безопасность

### Аутентификация
- **Метод**: Session-based
- **Хранение паролей**: SHA256 хеширование
- **Сессии**: Server-side (in-memory)
- **Время сессии**: 20 минут неактивности

### CSRF Protection
- Встроенная в ASP.NET Core
- Auto-token generation в Razor views

### Input Validation
- Server-side валидация
- Client-side (HTML5 attributes)

### Authorization
- Role-based access control (RBAC)
- Проверка авторизации на действия

---

## 🔄 Data Flow

### Жизненный цикл запроса

```
1. HTTP Request
   ↓
2. Routing (сопоставление URL маршруту)
   ↓
3. Controller Action (обработка запроса)
   ↓
4. EF Core Query (получение/обновление данных)
   ↓
5. PostgreSQL Query (SQL запрос к БД)
   ↓
6. Database Response (результаты БД)
   ↓
7. View Rendering (формирование HTML)
   ↓
8. HTTP Response (отправка клиенту)
```

---

## 📊 Диаграмма моделей EF Core

### Основные отношения:

```
User
 ├─ Customer (1:1)
 ├─ Cart (1:many) [авторизованные]
 └─ Order (1:many)

Product
 ├─ Category (many:1)
 ├─ Brand (many:1)
 ├─ ProductImage (1:many)
 ├─ ProductAttribute (1:many)
 └─ OrderItem (1:many)

Order
 ├─ Customer (many:1)
 ├─ OrderItem (1:many)
 ├─ Payment (1:many)
 ├─ Delivery (1:many)
 └─ Discount (many:1)

Cart
 ├─ Customer (many:1)
 └─ CartItem (1:many)
```

---

## 🎨 Frontend стек

### HTML Structure
- Bootstrap 5 компоненты
- Semantic HTML
- Accessible markup

### CSS Styling
- Bootstrap 5 classes
- Custom CSS (в `_Layout.cshtml`)
- Gradients и animations
- Responsive design (mobile-first)

### JavaScript
- **Fetch API** для AJAX запросов
- Vanilla JS (без фреймворков)
- Event listeners
- DOM manipulation

### Emojis
- Используются для визуализации
- Поддерживаются всеми современными браузерами

---

## 🚀 Performance Considerations

### Optimizations
- EF Core lazy loading (но используется eager loading с `.Include()`)
- Session compression
- Static file caching (в production)
- Database indexes на часто используемые поля

### Potential Improvements
- Асинхронные операции (async/await)
- Кеширование результатов запросов
- Pagination для больших наборов данных
- CDN для статических файлов
- Database query optimization

---

## 🧪 Testing Stack

### Unit Testing (готово к интеграции)
- xUnit или NUnit
- Моки для БД
- Test fixtures

### Integration Testing
- In-memory database для тестирования EF Core
- TestContainers для PostgreSQL

### Manual Testing
- Swagger/OpenAPI (опционально)
- Postman collections

---

## 📦 Структура проекта

```
cursovaia2/
├── Controllers/                    # MVC Controllers
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   ├── CheckoutController.cs
│   └── TestController.cs
│
├── Views/                          # Razor Views
│   ├── Home/
│   ├── Account/
│   ├── Product/
│   ├── Cart/
│   ├── Checkout/
│   ├── Shared/
│   │   └── _Layout.cshtml
│   └── _ViewImports.cshtml
│
├── Models/                         # View Models
│   ├── Cart.cs
│   └── CartItem.cs
│
├── ModelsDb/                       # EF Core Models (18 файлов)
│   ├── UserDb.cs
│   ├── CustomerDb.cs
│   ├── ProductDb.cs
│   ├── OrderDb.cs
│   └── ... (14 других)
│
├── Data/                           # Database Context
│   └── ApplicationDbContext.cs
│
├── Services/                       # Extensions & Utilities
│   └── SessionExtensions.cs
│
├── Migrations/                     # EF Core Migrations
│   ├── 20260514172125_InitialCreate.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── wwwroot/                        # Static Files
│   ├── css/
│   ├── js/
│   └── lib/ (Bootstrap via CDN)
│
├── Program.cs                      # Application Startup
├── appsettings.json               # Configuration
├── appsettings.Development.json    # Dev Configuration
│
└── cursovaia2.csproj              # Project File
```

---

## 📋 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Host=localhost;Port=5432;Database=Curs315;Username=postgres;Password=58194;"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft": "Warning"
	}
  }
}
```

### Program.cs
```csharp
// Services
builder.Services.AddDbContext<ApplicationDbContext>(...);
builder.Services.AddSession(...);
builder.Services.AddMvc();

// Pipeline
app.UseRouting();
app.UseSession();
app.MapControllers();
```

---

## 🔧 Инструменты разработки

### IDEs (рекомендуемые)
- Visual Studio 2026+
- Visual Studio Code с C# extensions
- JetBrains Rider

### Command Line Tools
```bash
# dotnet CLI
dotnet --version
dotnet new mvc
dotnet run
dotnet build
dotnet publish

# Entity Framework
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef database drop
```

### Database Tools
- pgAdmin 4 (PostgreSQL GUI)
- DBeaver (Universal DB tool)
- psql (PostgreSQL CLI)

### Browser DevTools
- Chrome DevTools (F12)
- Firefox Developer Edition
- Network tab для AJAX debugging

---

## 📚 Версия .NET 10 - Новые особенности

### Используемые
- Top-level statements
- Records (опционально для models)
- Nullable reference types
- Pattern matching
- LINQ improvements

### C# 13 Features
- File-scoped types
- Expression-bodied members
- Init-only properties
- Record types

---

## 🌐 Browser Support

### Минимальные требования
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Mobile browsers (iOS Safari 14+, Chrome Android 90+)

### JavaScript Features Used
- Fetch API (ES6)
- async/await
- Template literals
- Arrow functions
- Spread operator

---

## 📈 Scalability

### Текущее состояние
- Single server deployment
- In-memory sessions
- Single database connection

### Scaling Strategies (будущее)
- Distributed caching (Redis)
- Session storage in database
- Load balancing
- Database replication
- Microservices architecture

---

## 🔄 CI/CD Ready

### GitHub Actions (готово к интеграции)
```yaml
# .github/workflows/build.yml
- Checkout code
- Setup .NET
- Restore dependencies
- Build
- Run tests
- Deploy
```

### Docker Support (опционально)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY . .
RUN dotnet publish -o /app/publish
ENTRYPOINT ["dotnet", "/app/publish/cursovaia2.dll"]
```

---

## 📝 Документация и ссылки

### Официальная документация
- [Microsoft Docs - ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [PostgreSQL Documentation](https://www.postgresql.org/docs)
- [Bootstrap Documentation](https://getbootstrap.com/docs)

### Useful Resources
- [ASP.NET Core Security](https://docs.microsoft.com/aspnet/core/security)
- [Entity Framework Core Query Best Practices](https://docs.microsoft.com/ef/core/performance)

---

## ✅ Проверка всех компонентов

```bash
# Проверить версию .NET
dotnet --version            # 10.0.x

# Проверить пакеты
dotnet list package         # Показать все пакеты

# Проверить EF Core версию
dotnet ef --version         # 10.0.x

# Проверить подключение БД
dotnet ef database info     # Information about the database
```

---

## 🎯 Технологический стек - итоговая таблица

| Компонент | Технология | Версия |
|-----------|-----------|--------|
| Runtime | .NET | 10.0 |
| Language | C# | 13.0 |
| Web Framework | ASP.NET Core MVC | 10.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Database | PostgreSQL | 15+ |
| DB Driver | Npgsql | 9.0.1 |
| UI Framework | Bootstrap | 5.3.0 |
| Client-side | JavaScript (Vanilla) | ES6+ |
| View Engine | Razor | встроенно |
| Session Storage | In-memory | встроенно |
| Password Hashing | SHA256 | встроенно |

---

**Все компоненты установлены и готовы к использованию! 🚀**
