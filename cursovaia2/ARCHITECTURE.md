# Архитектура Stationery Store

## Обзор

Приложение следует классической архитектуре **Model-View-Controller (MVC)** с чистой структурой, готовой к расширению.

## Слои приложения

### 1. Presentation Layer (Представление)
**Расположение**: `Views/`

Отвечает за отображение данных пользователю.

#### Главные Views:
- **Home/Index.cshtml** - Главная страница с категориями и популярными товарами
- **Home/About.cshtml** - Информация о магазине
- **Home/Contacts.cshtml** - Контактная информация
- **Product/Catalog.cshtml** - Каталог товаров с фильтрацией и сортировкой
- **Product/Detail.cshtml** - Подробная информация о товаре
- **Cart/Index.cshtml** - Содержимое корзины
- **Cart/Checkout.cshtml** - Оформление заказа
- **Cart/OrderConfirmed.cshtml** - Подтверждение заказа
- **Shared/_Layout.cshtml** - Главный шаблон со стилями

#### Особенности:
- Встроенный CSS для минималистичного дизайна
- Адаптивный дизайн (Mobile-first)
- Razor синтаксис для динамического содержимого
- Иконки Unicode для визуального привлечения

### 2. Controllers Layer (Контроллеры)
**Расположение**: `Controllers/`

Обрабатывает HTTP-запросы и возвращает представления.

#### HomeController.cs
```csharp
- Index()          // Главная страница
- About()          // О магазине
- Contacts()       // Контакты
- Error()          // Обработка ошибок
```

**Методы помощников**:
- `GetCategories()` - Возвращает список категорий
- `GetPopularProducts()` - Возвращает популярные товары

#### ProductController.cs
```csharp
- Catalog()        // Каталог с фильтрацией и сортировкой
- Detail(id)       // Подробная информация о товаре
```

**Фильтрация**: По категориям через query parameter
**Сортировка**: По названию, цене (возрастание/убывание)

#### CartController.cs
```csharp
- Index()                    // Просмотр корзины
- AddToCart(productId)      // Добавление товара в корзину
- RemoveItem(cartItemId)    // Удаление товара из корзины
- UpdateQuantity(...)       // Изменение количества
- Clear()                   // Очистка корзины
- Checkout()                // Оформление заказа
- ProcessOrder()            // Обработка заказа
- OrderConfirmed()          // Подтверждение
```

**Работа с Session**: 
- Корзина хранится в `HttpContext.Session`
- Использует расширения `SessionExtensions`

### 3. Models Layer (Модели)
**Расположение**: `Models/`

Представляет данные приложения.

#### Product.cs
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
    public int Stock { get; set; }
}
```

#### Cart.cs
```csharp
public class Cart
{
    public List<CartItem> Items { get; set; }
    public int ItemCount { get; }          // Общее количество товаров
    public decimal Total { get; }          // Общая стоимость

    public void AddItem(Product product, int quantity)
    public void RemoveItem(int cartItemId)
    public void UpdateQuantity(int cartItemId, int quantity)
    public void Clear()
}
```

#### CartItem.cs
```csharp
public class CartItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; }
    public decimal Total { get; }          // Price * Quantity
}
```

#### Category.cs
```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }       // Unicode иконка
}
```

### 4. Services Layer (Сервисы)
**Расположение**: `Services/`

Вспомогательные функции и расширения.

#### SessionExtensions.cs
```csharp
// Расширения для сериализации объектов в сессии
public static void Set<T>(this ISession session, string key, T value)
public static T? Get<T>(this ISession session, string key)
```

**Функциональность**:
- Сериализация в JSON
- Десериализация из JSON
- Удобная работа со сложными типами данных

## Data Flow (Поток данных)

```
User Request
    ↓
[Router] → определяет контроллер и action
    ↓
[Controller] → обрабатывает запрос
    ↓
[Model] → подготавливает данные (жёсткий код)
    ↓
[View] → отрисовывает HTML с CSS
    ↓
[HTTP Response] → отправляет пользователю
```

## Управление состоянием

### Session для Корзины
```
User → Request → CartController.AddToCart()
                    ↓
                Session.Get<Cart>("cart")
                    ↓
                Если null → создать новую Cart
                    ↓
                cart.AddItem(product)
                    ↓
                Session.Set("cart", cart)
                    ↓
                Сохраняется как JSON строка
```

## Расширяемость

### Готово для подключения БД

1. **Создать DbContext**
```csharp
public class StoreDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    // ...
}
```

2. **Заменить GetAllProducts() на:**
```csharp
private List<Product> GetAllProducts()
{
    return _context.Products.ToList();
}
```

3. **Добавить Dependency Injection**
```csharp
public ProductController(StoreDbContext context)
{
    _context = context;
}
```

### Готово для API

Контроллеры можно легко преобразовать в API контроллеры с JSON ответами:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductApiController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts() 
        => Ok(_products);
}
```

### Готово для Аутентификации

```csharp
public class OrdersController : ControllerBase
{
    [Authorize]
    [HttpPost]
    public IActionResult CreateOrder(OrderDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier);
        // ...
    }
}
```

## Особенности дизайна

### Color Scheme
- **Primary**: #2563eb (Индиго синий)
- **Secondary**: #e2e8f0 (Светлый серый)
- **Gradient**: #667eea → #764ba2 (Фиолетовый градиент)
- **Background**: #f8f9fa (Белый с оттенком)
- **Text**: #1e293b (Тёмно-серый)

### Компоненты
- **Card**: Округлённые углы, мягкие тени, hover эффекты
- **Buttons**: Три варианта (primary, secondary, danger)
- **Inputs**: Простые и чистые
- **Tables**: Минималистичные с хорошей читаемостью
- **Forms**: Вертикальное расположение с метками

### Адаптивность
- **Grid System**: CSS Grid с auto-fit
- **Mobile Breakpoint**: 768px
- **Flexible Layout**: Использует flexbox и grid
- **Responsive Typography**: Масштабируется с размером экрана

## Стандарты кодирования

- **Именование**: camelCase для переменных, PascalCase для классов
- **Структура**: Чистые, читаемые методы
- **Комментарии**: Минимум, только где необходимо
- **Организация**: Группировка связанной функциональности

## Performance Considerations

1. **Жёсткие данные**: Подходит для учебных целей
2. **Кэширование**: Готово для добавления
3. **Async/Await**: Готово для асинхронных операций
4. **Pagination**: Можно добавить для больших каталогов
5. **Lazy Loading**: Можно применить для изображений

## Security

1. **HTTPS**: По умолчанию в production
2. **CSRF Protection**: Встроено в ASP.NET Core MVC
3. **Input Validation**: Готово для добавления
4. **Authentication**: Архитектура готова для [Authorize]
5. **Authorization**: Роли и claims готовы

## Заключение

Архитектура Stationery Store демонстрирует чистые принципы MVC разработки с:
- Четкой разделением ответственности
- Готовностью к масштабированию
- Правильной структурой для переходу на БД
- Профессиональным дизайном
- Современными веб-технологиями
