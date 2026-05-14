# Руководство разработчика - Stationery Store

## Начало работы

### Структура папок

```
cursovaia2/
├── Controllers/          # Обработка запросов
├── Models/              # Структуры данных
├── Services/            # Вспомогательные сервисы
├── Views/               # Шаблоны HTML
├── Properties/          # Конфигурация проекта
├── Program.cs           # Точка входа приложения
├── cursovaia2.csproj    # Конфигурация проекта
├── README.md            # Документация
└── ARCHITECTURE.md      # Описание архитектуры
```

## Добавление нового товара в каталог

### Шаг 1: Найти метод GetAllProducts()

**Файл**: `Controllers/ProductController.cs`

```csharp
private List<Product> GetAllProducts()
{
    return new List<Product>
    {
        // Существующие товары...
        new() { 
            Id = 25, 
            Name = "Новый товар", 
            Category = "Ручки", 
            Price = 50, 
            Description = "Описание товара", 
            ImageUrl = "/images/product.jpg", 
            Stock = 100 
        }
    };
}
```

### Шаг 2: Добавить товар

```csharp
new() { 
    Id = 25, 
    Name = "Премиум гелевая ручка", 
    Category = "Ручки", 
    Price = 120, 
    Description = "Люксовая гелевая ручка с гладким письмом", 
    ImageUrl = "/images/premium-pen.jpg", 
    Stock = 50 
}
```

### Важно: Id должен быть уникальным!

## Добавление новой категории

### Шаг 1: Найти метод GetCategories()

**Файл**: `Controllers/HomeController.cs`, `Controllers/ProductController.cs`

```csharp
private List<Category> GetCategories()
{
    return new List<Category>
    {
        new() { Id = 1, Name = "Ручки", Icon = "✏️" },
        new() { Id = 2, Name = "Карандаши", Icon = "📝" },
        // Добавить новую категорию:
        new() { Id = 7, Name = "Красители", Icon = "🎨" }
    };
}
```

## Создание новой страницы

### Пример: Добавить страницу "Доставка"

### Шаг 1: Создать action в HomeController

**Файл**: `Controllers/HomeController.cs`

```csharp
public IActionResult Delivery()
{
    return View();
}
```

### Шаг 2: Создать файл представления

**Файл**: `Views/Home/Delivery.cshtml`

```html
@{
    ViewData["Title"] = "Доставка";
}

<div style="max-width: 800px; margin: 3rem auto;">
    <h1 style="margin-bottom: 2rem; color: #1e293b;">Доставка</h1>

    <div style="background: white; padding: 2rem; border-radius: 12px; 
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);">
        <h2 style="color: #2563eb; margin-bottom: 1rem;">Способы доставки</h2>
        <p>Информация о доставке...</p>
    </div>
</div>
```

### Шаг 3: Добавить ссылку в навигацию

**Файл**: `Views/Shared/_Layout.cshtml`

```html
<nav>
    <a href="/">Главная</a>
    <a href="/Product/Catalog">Каталог</a>
    <a href="/Home/Delivery">Доставка</a>  <!-- Новая ссылка -->
    <a href="/Home/About">О магазине</a>
    <a href="/Home/Contacts">Контакты</a>
</nav>
```

## Изменение дизайна

### Изменить основной цвет

**Файл**: `Views/Shared/_Layout.cshtml` (строка ~80-150)

```css
/* Было */
color: #2563eb;

/* Стало */
color: #059669;  /* Новый зелёный цвет */
```

### Изменить шрифт

```html
<head>
    <style>
        body {
            /* Было */
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto;

            /* Стало */
            font-family: 'Georgia', serif;
        }
    </style>
</head>
```

### Добавить новый цвет

```css
/* В начало блока стилей */
:root {
    --primary: #2563eb;
    --secondary: #e2e8f0;
    --success: #10b981;
    --danger: #ef4444;
}

/* Использование */
color: var(--primary);
```

## Работа с корзиной (Session)

### Получить корзину

```csharp
var cart = HttpContext.Session.Get<Cart>("cart");
```

### Добавить товар

```csharp
var product = new Product { Id = 1, Name = "Ручка", Price = 25 };
cart.AddItem(product, quantity: 1);
HttpContext.Session.Set("cart", cart);
```

### Получить итоговую стоимость

```csharp
decimal total = cart.Total;  // Автоматически расчитывается
```

## Подключение базы данных (в будущем)

### Шаг 1: Установить NuGet пакеты

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Шаг 2: Создать DbContext

**Файл**: `Data/StoreDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using cursovaia2.Models;

namespace cursovaia2.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}
```

### Шаг 3: Обновить Program.cs

```csharp
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Шаг 4: Обновить контроллер

```csharp
private readonly StoreDbContext _context;

public ProductController(StoreDbContext context)
{
    _context = context;
}

private List<Product> GetAllProducts()
{
    return _context.Products.ToList();  // Вместо жёстких данных
}
```

## Типичные ошибки и их решения

### Ошибка: "View not found"

**Причина**: Файл .cshtml создан в неправильной папке

**Решение**: 
- Для `HomeController.Index()` → `Views/Home/Index.cshtml`
- Для `ProductController.Catalog()` → `Views/Product/Catalog.cshtml`

### Ошибка: "ISession не содержит определение для Get"

**Причина**: Неправильное использование Session

**Решение**: Использовать расширения `SessionExtensions`

```csharp
using cursovaia2.Services;

var cart = HttpContext.Session.Get<Cart>("cart");  // ✓ Правильно
```

### Ошибка: "Model is null"

**Причина**: Представление передаёт null вместо модели

**Решение**: Проверить null перед использованием

```csharp
@if (Model != null)
{
    // Использовать Model
}
else
{
    <p>Данные не загружены</p>
}
```

## Debug и Testing

### Просмотр Session данных

```csharp
public IActionResult DebugCart()
{
    var cart = HttpContext.Session.Get<Cart>("cart");
    System.Diagnostics.Debug.WriteLine($"Cart items: {cart?.Items.Count}");
    return Ok(cart);
}
```

### Проверка маршрутов

```csharp
// Program.cs
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
```

## Performance Tips

### Кэширование данных

```csharp
private static List<Product> _cachedProducts;

private List<Product> GetAllProducts()
{
    if (_cachedProducts == null)
    {
        _cachedProducts = LoadProducts();
    }
    return _cachedProducts;
}
```

### Асинхронные операции

```csharp
public async Task<IActionResult> Index()
{
    var products = await Task.FromResult(GetAllProducts());
    return View(products);
}
```

## Утилиты и Helpers

### HTML Helper для цены

```html
@helper string FormatPrice(decimal price)
{
    @:₽@price.ToString("F2")
}

<!-- Использование -->
@FormatPrice(product.Price)
```

### Получить количество товаров в корзине

```html
@{
    var cart = HttpContext.Session.Get<Cart>("cart");
    var count = cart?.ItemCount ?? 0;
}

<span class="cart-count">@count</span>
```

## Комманды для разработки

```bash
# Восстановить пакеты
dotnet restore

# Собрать проект
dotnet build

# Запустить проект
dotnet run

# Очистить проект
dotnet clean

# Просмотреть потребления памяти (debug)
# Добавить в Program.cs:
// app.UseStatusCodePages();
```

## Полезные ресурсы

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [CSS Grid Layout](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Grid_Layout)
- [Razor Syntax](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor)

## Контрольный список при добавлении новой функции

- [ ] Создан контроллер / action
- [ ] Создано представление
- [ ] Добавлены маршруты в меню
- [ ] Проверена передача данных Model → View
- [ ] Добавлены стили (если нужны)
- [ ] Проект успешно собран (dotnet build)
- [ ] Проверена адаптивность на мобильных
- [ ] Добавлены обработчики ошибок
- [ ] Обновлена документация

## Лучшие практики

✅ **Делайте**:
- Используйте паттерн MVC
- Держите контроллеры лёгкими
- Используйте встроенные ASP.NET Core сервисы
- Документируйте сложный код
- Тестируйте регулярно

❌ **Избегайте**:
- Логику БД в контроллерах
- Жёсткий код в Views
- Дублирование кода
- Очень большие методы
- Несогласованность стилей

---

**Версия**: 1.0  
**Последнее обновление**: 2024
