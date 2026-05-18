using cursovaia2.Data;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CheckDatabase()
        {
            try
            {
                if (!_context.Database.CanConnect())
                    return Content("❌ Не удаётся подключиться к базе данных!");

                var result = "<h2>✅ Подключение к БД успешно!</h2><hr><h3>📊 Таблицы:</h3><ul>";
                result += $"<li>Ролей: {_context.Roles.Count()}</li>";
                result += $"<li>Пользователей: {_context.Users.Count()}</li>";
                result += $"<li>Покупателей: {_context.Customers.Count()}</li>";
                result += $"<li>Категорий: {_context.CategoriesDb.Count()}</li>";
                result += $"<li>Брендов: {_context.Brands.Count()}</li>";
                result += $"<li>Товаров: {_context.ProductsDb.Count()}</li>";
                result += $"<li>Заказов: {_context.Orders.Count()}</li>";
                result += $"<li>История статусов: {_context.OrderStatusHistories.Count()}</li>";
                result += $"<li>Платежей: {_context.Payments.Count()}</li>";
                result += $"<li>Доставок: {_context.Deliveries.Count()}</li>";
                result += $"<li>Способов оплаты: {_context.PaymentMethods.Count()}</li>";
                result += $"<li>Способов доставки: {_context.DeliveryMethods.Count()}</li>";
                result += $"<li>Скидок: {_context.Discounts.Count()}</li>";
                result += $"<li>Промокодов: {_context.PromoCodes.Count()}</li>";
                result += $"<li>Корзин: {_context.Carts.Count()}</li>";
                result += $"<li>Отзывов: {_context.Reviews.Count()}</li>";
                result += $"<li>Избранного: {_context.Wishlists.Count()}</li>";
                result += "</ul><p><a href='/Admin'>Админ-панель</a> | <a href='/'>Главная</a></p>";
                return Content(result, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content($"❌ Ошибка: {ex.Message}", "text/html; charset=utf-8");
            }
        }

        public IActionResult AddTestData()
        {
            try
            {
                EnsureRoles();
                EnsureAdminUser();
                EnsureReferenceData();

                var productsAdded = 0;
                if (!_context.ProductsDb.Any())
                    productsAdded = SeedSampleProducts();

                var msg = "<h2>✅ Данные подготовлены</h2><ul>";
                msg += "<li>Роли: admin, user</li>";
                msg += "<li><strong>Админ:</strong> admin@stationery.com / admin123</li>";
                msg += "<li><strong>Пользователь:</strong> user@stationery.com / user123</li>";
                msg += "<li>Способы оплаты и доставки</li>";
                if (productsAdded > 0) msg += $"<li>Добавлено товаров: {productsAdded}</li>";
                msg += "</ul>";
                msg += "<p><a href='/Admin'>⚙️ Админ-панель</a> | <a href='/Account/Login'>Вход</a> | <a href='/Product/Catalog'>Каталог</a></p>";
                return Content(msg, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content($"❌ Ошибка: {ex.Message}<br>{ex.InnerException?.Message}", "text/html; charset=utf-8");
            }
        }

        private void EnsureRoles()
        {
            if (!_context.Roles.Any(r => r.Name == AuthSession.AdminRoleName))
                _context.Roles.Add(new RoleDb { Name = AuthSession.AdminRoleName });
            if (!_context.Roles.Any(r => r.Name == AuthSession.UserRoleName))
                _context.Roles.Add(new RoleDb { Name = AuthSession.UserRoleName });
            _context.SaveChanges();
        }

        private void EnsureAdminUser()
        {
            var adminRole = _context.Roles.First(r => r.Name == AuthSession.AdminRoleName);
            var userRole = _context.Roles.First(r => r.Name == AuthSession.UserRoleName);

            if (!_context.Users.Any(u => u.Email == "admin@stationery.com"))
            {
                var admin = new UserDb
                {
                    Email = "admin@stationery.com",
                    PasswordHash = PasswordHasher.Hash("admin123"),
                    RoleId = adminRole.Id,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(admin);
                _context.SaveChanges();
            }

            if (!_context.Users.Any(u => u.Email == "user@stationery.com"))
            {
                var user = new UserDb
                {
                    Email = "user@stationery.com",
                    PasswordHash = PasswordHasher.Hash("user123"),
                    RoleId = userRole.Id,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                _context.Customers.Add(new CustomerDb
                {
                    UserId = user.Id,
                    Name = "Тестовый покупатель",
                    Phone = "+7-999-123-45-67",
                    BonusBalance = 100
                });
                _context.SaveChanges();
            }
        }

        private void EnsureReferenceData()
        {
            ReferenceDataSeeder.EnsureDeliveryAndPaymentMethods(_context);
        }

        private int SeedSampleProducts()
        {
            var category = _context.CategoriesDb.FirstOrDefault();
            if (category == null)
            {
                category = new CategoryDb { Name = "Канцелярия" };
                _context.CategoriesDb.Add(category);
                _context.SaveChanges();
            }

            var brand = _context.Brands.FirstOrDefault();
            if (brand == null)
            {
                brand = new BrandDb { Name = "ErichKrause" };
                _context.Brands.Add(brand);
                _context.SaveChanges();
            }

            var products = new List<ProductDb>
            {
                new() { Name = "Ручка гелевая синяя", Sku = "PEN-001", CategoryId = category.Id, BrandId = brand.Id, Price = 25m, CostPrice = 10m, Description = "Гелевая ручка", IsActive = true },
                new() { Name = "Карандаш HB", Sku = "PENCIL-001", CategoryId = category.Id, BrandId = brand.Id, Price = 15m, CostPrice = 5m, Description = "Карандаш для письма", IsActive = true },
                new() { Name = "Тетрадь 48 л.", Sku = "NOTE-001", CategoryId = category.Id, BrandId = brand.Id, Price = 45m, CostPrice = 20m, Description = "Тетрадь в линейку", IsActive = true }
            };

            _context.ProductsDb.AddRange(products);
            _context.SaveChanges();
            return products.Count;
        }
    }
}
