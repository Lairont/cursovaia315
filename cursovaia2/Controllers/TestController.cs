using cursovaia2.Data;
using cursovaia2.ModelsDb;
using Microsoft.AspNetCore.Mvc;

namespace cursovaia2.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Test/CheckDatabase
        public IActionResult CheckDatabase()
        {
            try
            {
                // Проверяем подключение к БД
                var canConnect = _context.Database.CanConnect();

                if (!canConnect)
                {
                    return Content("❌ Ошибка: Не удаётся подключиться к базе данных!");
                }

                var result = "<h2>✅ Подключение к БД успешно!</h2>";
                result += "<hr>";

                // Получаем информацию о таблицах
                result += "<h3>📊 Информация о таблицах:</h3>";
                result += "<ul>";
                result += $"<li>Ролей в БД: {_context.Roles.Count()}</li>";
                result += $"<li>Пользователей в БД: {_context.Users.Count()}</li>";
                result += $"<li>Покупателей в БД: {_context.Customers.Count()}</li>";
                result += $"<li>Категорий в БД: {_context.CategoriesDb.Count()}</li>";
                result += $"<li>Брендов в БД: {_context.Brands.Count()}</li>";
                result += $"<li>Товаров в БД: {_context.ProductsDb.Count()}</li>";
                result += $"<li>Заказов в БД: {_context.Orders.Count()}</li>";
                result += $"<li>Платежей в БД: {_context.Payments.Count()}</li>";
                result += $"<li>Доставок в БД: {_context.Deliveries.Count()}</li>";
                result += $"<li>Скидок в БД: {_context.Discounts.Count()}</li>";
                result += $"<li>Промокодов в БД: {_context.PromoCodes.Count()}</li>";
                result += $"<li>Корзин в БД: {_context.Carts.Count()}</li>";
                result += $"<li>Отзывов в БД: {_context.Reviews.Count()}</li>";
                result += $"<li>Вишлистов в БД: {_context.Wishlists.Count()}</li>";
                result += "</ul>";

                return Content(result, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content($"❌ Ошибка подключения: {ex.Message}", "text/html; charset=utf-8");
            }
        }

        // GET: Test/AddTestData
        public IActionResult AddTestData()
        {
            try
            {
                // Проверяем, есть ли уже тестовые данные
                if (_context.Roles.Any())
                {
                    return Content("⚠️ Тестовые данные уже добавлены!", "text/html; charset=utf-8");
                }

                // Добавляем роли
                var adminRole = new RoleDb { Name = "admin" };
                var userRole = new RoleDb { Name = "user" };
                _context.Roles.Add(adminRole);
                _context.Roles.Add(userRole);
                _context.SaveChanges();

                // Добавляем пользователя
                var user = new UserDb
                {
                    Email = "test@example.com",
                    PasswordHash = "testhash123",
                    RoleId = userRole.Id,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                // Добавляем покупателя
                var customer = new CustomerDb
                {
                    UserId = user.Id,
                    Name = "Тестовый покупатель",
                    Phone = "+7-999-123-45-67",
                    BonusBalance = 100
                };
                _context.Customers.Add(customer);
                _context.SaveChanges();

                // Добавляем категорию
                var category = new CategoryDb
                {
                    Name = "Тестовая категория",
                    ParentId = null
                };
                _context.CategoriesDb.Add(category);
                _context.SaveChanges();

                // Добавляем бренд
                var brand = new BrandDb
                {
                    Name = "Тестовый бренд"
                };
                _context.Brands.Add(brand);
                _context.SaveChanges();

                // Добавляем товары
                var products = new List<ProductDb>
                {
                    new ProductDb
                    {
                        Name = "Шариковая ручка синяя",
                        Sku = "PEN-001",
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Price = 25m,
                        CostPrice = 10m,
                        Description = "Удобная шариковая ручка с синей пастой",
                        IsActive = true
                    },
                    new ProductDb
                    {
                        Name = "Карандаш HB",
                        Sku = "PENCIL-001",
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Price = 15m,
                        CostPrice = 5m,
                        Description = "Классический карандаш для письма",
                        IsActive = true
                    },
                    new ProductDb
                    {
                        Name = "Тетрадь 48 листов",
                        Sku = "NOTE-001",
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Price = 45m,
                        CostPrice = 20m,
                        Description = "Качественная тетрадь с белой бумагой",
                        IsActive = true
                    },
                    new ProductDb
                    {
                        Name = "Блокнот премиум",
                        Sku = "BLOCK-001",
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Price = 85m,
                        CostPrice = 40m,
                        Description = "Стильный блокнот премиум класса",
                        IsActive = true
                    },
                    new ProductDb
                    {
                        Name = "Клей ПВА 100ml",
                        Sku = "GLUE-001",
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Price = 35m,
                        CostPrice = 15m,
                        Description = "Универсальный клей для бумаги",
                        IsActive = true
                    }
                };

                foreach (var p in products)
                {
                    _context.ProductsDb.Add(p);
                }
                _context.SaveChanges();

                var result = "<h2>✅ Тестовые данные успешно добавлены!</h2>";
                result += "<hr>";
                result += "<h3>📝 Добавленные данные:</h3>";
                result += "<ul>";
                result += $"<li>✓ Роль 'admin'</li>";
                result += $"<li>✓ Роль 'user'</li>";
                result += $"<li>✓ Пользователь: test@example.com</li>";
                result += $"<li>✓ Покупатель: Тестовый покупатель</li>";
                result += $"<li>✓ Категория: Тестовая категория</li>";
                result += $"<li>✓ Бренд: Тестовый бренд</li>";
                result += $"<li>✓ Добавлено товаров: {products.Count} шт.</li>";
                result += "</ul>";
                result += "<hr>";
                result += "<h3>🛍️ Навигация:</h3>";
                result += "<ul>";
                result += $"<li><a href='/Product/Catalog'>🛒 Перейти в каталог товаров</a></li>";
                result += $"<li><a href='/Test/CheckDatabase'>📊 Проверить БД</a></li>";
                result += $"<li><a href='/'>🏠 На главную</a></li>";
                result += "</ul>";

                return Content(result, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content($"❌ Ошибка при добавлении данных: {ex.Message}", "text/html; charset=utf-8");
            }
        }
    }
}
