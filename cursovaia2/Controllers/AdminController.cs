using cursovaia2.Data;
using cursovaia2.Filters;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] OrderStatuses =
            { "pending", "paid", "shipped", "completed", "cancelled" };

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["AdminLayout"] = true;
            var model = new AdminDashboardViewModel
            {
                Categories = _context.CategoriesDb.Count(),
                Brands = _context.Brands.Count(),
                Products = _context.ProductsDb.Count(),
                Users = _context.Users.Count(),
                Customers = _context.Customers.Count(),
                Orders = _context.Orders.Count(),
                Reviews = _context.Reviews.Count(),
                Wishlists = _context.Wishlists.Count(),
                Carts = _context.Carts.Count(),
                Discounts = _context.Discounts.Count(),
                PromoCodes = _context.PromoCodes.Count(),
                Payments = _context.Payments.Count(),
                Deliveries = _context.Deliveries.Count(),
                PaymentMethods = _context.PaymentMethods.Count(),
                DeliveryMethods = _context.DeliveryMethods.Count(),
                ProductDiscounts = _context.ProductDiscounts.Count()
            };
            return View(model);
        }

        #region Categories

        public IActionResult Categories()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Категории";
            return View(_context.CategoriesDb.Include(c => c.Parent).OrderBy(c => c.Name).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCategory(int? id, string name, int? parentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction(nameof(Categories));

            if (id.HasValue)
            {
                var cat = _context.CategoriesDb.Find(id.Value);
                if (cat != null)
                {
                    cat.Name = name.Trim();
                    cat.ParentId = parentId;
                }
            }
            else
            {
                _context.CategoriesDb.Add(new CategoryDb { Name = name.Trim(), ParentId = parentId });
            }

            _context.SaveChanges();
            TempData["Success"] = "Категория сохранена";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            var cat = _context.CategoriesDb.Find(id);
            if (cat != null)
            {
                _context.CategoriesDb.Remove(cat);
                _context.SaveChanges();
                TempData["Success"] = "Категория удалена";
            }
            return RedirectToAction(nameof(Categories));
        }

        #endregion

        #region Brands

        public IActionResult Brands()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Бренды";
            return View(_context.Brands.OrderBy(b => b.Name).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveBrand(int? id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction(nameof(Brands));

            if (id.HasValue)
            {
                var brand = _context.Brands.Find(id.Value);
                if (brand != null) brand.Name = name.Trim();
            }
            else
            {
                _context.Brands.Add(new BrandDb { Name = name.Trim() });
            }

            _context.SaveChanges();
            TempData["Success"] = "Бренд сохранён";
            return RedirectToAction(nameof(Brands));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBrand(int id)
        {
            var brand = _context.Brands.Find(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Brands));
        }

        #endregion

        #region Products

        public IActionResult Products()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Товары";
            var products = _context.ProductsDb
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.Id)
                .ToList();
            return View(products);
        }

        public IActionResult ProductEdit(int? id)
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = id.HasValue ? "Редактировать товар" : "Новый товар";
            LoadProductLookups();

            if (!id.HasValue)
                return View(new ProductDb { IsActive = true });

            var product = _context.ProductsDb
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .FirstOrDefault(p => p.Id == id);
            return product == null ? NotFound() : View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveProduct(ProductDb model, bool isActive = false)
        {
            model.IsActive = isActive || Request.Form.ContainsKey("IsActive");
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Sku))
            {
                TempData["Error"] = "Название и SKU обязательны";
                return RedirectToAction(nameof(ProductEdit), new { id = model.Id });
            }

            if (model.Id > 0)
            {
                var existing = _context.ProductsDb.Find(model.Id);
                if (existing == null) return NotFound();
                existing.Name = model.Name.Trim();
                existing.Sku = model.Sku.Trim();
                existing.CategoryId = model.CategoryId;
                existing.BrandId = model.BrandId;
                existing.Price = model.Price;
                existing.CostPrice = model.CostPrice;
                existing.Description = model.Description;
                existing.IsActive = model.IsActive;
            }
            else
            {
                model.Name = model.Name.Trim();
                model.Sku = model.Sku.Trim();
                _context.ProductsDb.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Товар сохранён";
                return RedirectToAction(nameof(ProductEdit), new { id = model.Id });
            }

            _context.SaveChanges();
            TempData["Success"] = "Товар сохранён";
            return RedirectToAction(nameof(ProductEdit), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.ProductsDb.Find(id);
            if (product != null)
            {
                _context.ProductsDb.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductImage(int productId, string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                _context.ProductImages.Add(new ProductImageDb { ProductId = productId, Url = url.Trim() });
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ProductEdit), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductImage(int id, int productId)
        {
            var img = _context.ProductImages.Find(id);
            if (img != null) _context.ProductImages.Remove(img);
            _context.SaveChanges();
            return RedirectToAction(nameof(ProductEdit), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductAttribute(int productId, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                _context.ProductAttributes.Add(new ProductAttributeDb
                {
                    ProductId = productId,
                    Key = key.Trim(),
                    Value = value.Trim()
                });
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ProductEdit), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductAttribute(int id, int productId)
        {
            var attr = _context.ProductAttributes.Find(id);
            if (attr != null) _context.ProductAttributes.Remove(attr);
            _context.SaveChanges();
            return RedirectToAction(nameof(ProductEdit), new { id = productId });
        }

        #endregion

        #region Users & Roles

        public IActionResult Roles()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Роли";
            return View(_context.Roles.OrderBy(r => r.Name).ToList());
        }

        public IActionResult Users()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Пользователи";
            var users = _context.Users.Include(u => u.Role).OrderByDescending(u => u.Id).ToList();
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Name");
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveUser(int? id, string email, int? roleId, string status, string? password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction(nameof(Users));

            if (id.HasValue)
            {
                var user = _context.Users.Find(id.Value);
                if (user == null) return RedirectToAction(nameof(Users));
                user.Email = email.Trim();
                user.RoleId = roleId;
                user.Status = string.IsNullOrWhiteSpace(status) ? "active" : status;
                if (!string.IsNullOrWhiteSpace(password))
                    user.PasswordHash = PasswordHasher.Hash(password);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    TempData["Error"] = "Для нового пользователя нужен пароль";
                    return RedirectToAction(nameof(Users));
                }
                _context.Users.Add(new UserDb
                {
                    Email = email.Trim(),
                    RoleId = roleId,
                    Status = string.IsNullOrWhiteSpace(status) ? "active" : status,
                    PasswordHash = PasswordHasher.Hash(password),
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();
            TempData["Success"] = "Пользователь сохранён";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null && user.Id != HttpContext.Session.GetInt32("UserId"))
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Users));
        }

        public IActionResult Customers()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Покупатели";
            var list = _context.Customers.Include(c => c.User).OrderByDescending(c => c.Id).ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCustomer(int id, string name, string? phone, decimal bonusBalance)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null) return RedirectToAction(nameof(Customers));
            customer.Name = name.Trim();
            customer.Phone = phone;
            customer.BonusBalance = bonusBalance;
            _context.SaveChanges();
            TempData["Success"] = "Профиль покупателя обновлён";
            return RedirectToAction(nameof(Customers));
        }

        #endregion

        #region Orders

        public IActionResult Orders()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Заказы";
            var orders = _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            return View(orders);
        }

        public IActionResult OrderEdit(int id)
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = $"Заказ #{id}";
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Payments).ThenInclude(p => p.Method)
                .Include(o => o.Deliveries).ThenInclude(d => d.DeliveryMethod)
                .FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            ViewBag.Statuses = new SelectList(OrderStatuses, order.Status);
            ViewBag.History = _context.OrderStatusHistories
                .Where(h => h.OrderId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToList();
            ViewBag.Discounts = new SelectList(_context.Discounts.ToList(), "Id", "Name", order.DiscountId);
            ViewBag.PromoCodes = new SelectList(_context.PromoCodes.ToList(), "Id", "Code", order.PromoCodeId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int id, string status, string? comment)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return RedirectToAction(nameof(Orders));

            var oldStatus = order.Status;
            if (oldStatus != status && OrderStatuses.Contains(status))
            {
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;
                _context.OrderStatusHistories.Add(new OrderStatusHistoryDb
                {
                    OrderId = id,
                    OldStatus = oldStatus,
                    NewStatus = status,
                    ChangedByUserId = HttpContext.Session.GetInt32("UserId"),
                    ChangedAt = DateTime.UtcNow,
                    Comment = comment
                });
                _context.SaveChanges();
                TempData["Success"] = "Статус заказа обновлён";
            }

            return RedirectToAction(nameof(OrderEdit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveOrder(int id, int? discountId, int? promoCodeId, decimal finalPrice)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return RedirectToAction(nameof(Orders));
            order.DiscountId = discountId;
            order.PromoCodeId = promoCodeId;
            order.FinalPrice = finalPrice;
            order.TotalPrice = finalPrice;
            order.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            TempData["Success"] = "Заказ обновлён";
            return RedirectToAction(nameof(OrderEdit), new { id });
        }

        #endregion

        #region Discounts & Promo

        public IActionResult Discounts()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Скидки";
            return View(_context.Discounts.OrderByDescending(d => d.Id).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDiscount(int? id, string name, string type, decimal value,
            DateTime? startDate, DateTime? endDate)
        {
            if (id.HasValue)
            {
                var d = _context.Discounts.Find(id.Value);
                if (d != null)
                {
                    d.Name = name; d.Type = type; d.Value = value;
                    d.StartDate = startDate; d.EndDate = endDate;
                }
            }
            else
            {
                _context.Discounts.Add(new DiscountDb
                {
                    Name = name, Type = type, Value = value,
                    StartDate = startDate, EndDate = endDate
                });
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Discounts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDiscount(int id)
        {
            var d = _context.Discounts.Find(id);
            if (d != null) { _context.Discounts.Remove(d); _context.SaveChanges(); }
            return RedirectToAction(nameof(Discounts));
        }

        public IActionResult PromoCodes()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Промокоды";
            var list = _context.PromoCodes.Include(p => p.Discount).OrderByDescending(p => p.Id).ToList();
            ViewBag.Discounts = new SelectList(_context.Discounts.ToList(), "Id", "Name");
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePromoCode(int? id, string code, int discountId, int? usageLimit)
        {
            if (id.HasValue)
            {
                var p = _context.PromoCodes.Find(id.Value);
                if (p != null) { p.Code = code.Trim(); p.DiscountId = discountId; p.UsageLimit = usageLimit; }
            }
            else
            {
                _context.PromoCodes.Add(new PromoCodeDb
                {
                    Code = code.Trim(),
                    DiscountId = discountId,
                    UsageLimit = usageLimit,
                    CreatedAt = DateTime.UtcNow
                });
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(PromoCodes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePromoCode(int id)
        {
            var p = _context.PromoCodes.Find(id);
            if (p != null) { _context.PromoCodes.Remove(p); _context.SaveChanges(); }
            return RedirectToAction(nameof(PromoCodes));
        }

        public IActionResult ProductDiscounts()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Скидки на товары";
            var list = _context.ProductDiscounts
                .Include(pd => pd.Product)
                .Include(pd => pd.Discount)
                .ToList();
            ViewBag.Products = new SelectList(_context.ProductsDb.Select(p => new { p.Id, Name = p.Name }), "Id", "Name");
            ViewBag.Discounts = new SelectList(_context.Discounts.ToList(), "Id", "Name");
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveProductDiscount(int productId, int discountId)
        {
            if (productId <= 0 || discountId <= 0)
            {
                TempData["Error"] = "Выберите товар и скидку";
                return RedirectToAction(nameof(ProductDiscounts));
            }

            var existing = _context.ProductDiscounts.Where(pd => pd.ProductId == productId).ToList();
            if (existing.Any())
                _context.ProductDiscounts.RemoveRange(existing);

            _context.ProductDiscounts.Add(new ProductDiscountDb { ProductId = productId, DiscountId = discountId });
            _context.SaveChanges();
            TempData["Success"] = "Скидка привязана к товару";
            return RedirectToAction(nameof(ProductDiscounts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductDiscount(int id)
        {
            var pd = _context.ProductDiscounts.Find(id);
            if (pd != null) { _context.ProductDiscounts.Remove(pd); _context.SaveChanges(); }
            return RedirectToAction(nameof(ProductDiscounts));
        }

        #endregion

        #region Payments & Delivery

        public IActionResult PaymentMethods()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Способы оплаты";
            return View(_context.PaymentMethods.OrderBy(m => m.Name).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePaymentMethod(int? id, string name)
        {
            if (id.HasValue)
            {
                var m = _context.PaymentMethods.Find(id.Value);
                if (m != null) m.Name = name.Trim();
            }
            else
                _context.PaymentMethods.Add(new PaymentMethodDb { Name = name.Trim() });
            _context.SaveChanges();
            return RedirectToAction(nameof(PaymentMethods));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePaymentMethod(int id)
        {
            var m = _context.PaymentMethods.Find(id);
            if (m != null) { _context.PaymentMethods.Remove(m); _context.SaveChanges(); }
            return RedirectToAction(nameof(PaymentMethods));
        }

        public IActionResult Payments()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Платежи";
            var list = _context.Payments.Include(p => p.Order).Include(p => p.Method).OrderByDescending(p => p.Id).ToList();
            ViewBag.Orders = new SelectList(_context.Orders.Select(o => new { o.Id }), "Id", "Id");
            ViewBag.Methods = new SelectList(_context.PaymentMethods.ToList(), "Id", "Name");
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePayment(int? id, int orderId, decimal amount, int? methodId, string status)
        {
            if (id.HasValue)
            {
                var p = _context.Payments.Find(id.Value);
                if (p != null)
                {
                    p.OrderId = orderId; p.Amount = amount; p.MethodId = methodId; p.Status = status;
                    if (status == "paid") p.PaidAt = DateTime.UtcNow;
                }
            }
            else
            {
                _context.Payments.Add(new PaymentDb
                {
                    OrderId = orderId,
                    Amount = amount,
                    MethodId = methodId,
                    Status = status,
                    CreatedAt = DateTime.UtcNow,
                    PaidAt = status == "paid" ? DateTime.UtcNow : null
                });
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Payments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePayment(int id)
        {
            var p = _context.Payments.Find(id);
            if (p != null) { _context.Payments.Remove(p); _context.SaveChanges(); }
            return RedirectToAction(nameof(Payments));
        }

        public IActionResult DeliveryMethods()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Способы доставки";
            return View(_context.DeliveryMethods.OrderBy(m => m.Name).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDeliveryMethod(int? id, string name, decimal price)
        {
            if (id.HasValue)
            {
                var m = _context.DeliveryMethods.Find(id.Value);
                if (m != null) { m.Name = name.Trim(); m.Price = price; }
            }
            else
                _context.DeliveryMethods.Add(new DeliveryMethodDb { Name = name.Trim(), Price = price });
            _context.SaveChanges();
            return RedirectToAction(nameof(DeliveryMethods));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDeliveryMethod(int id)
        {
            var m = _context.DeliveryMethods.Find(id);
            if (m != null) { _context.DeliveryMethods.Remove(m); _context.SaveChanges(); }
            return RedirectToAction(nameof(DeliveryMethods));
        }

        public IActionResult Deliveries()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Доставки";
            var list = _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.DeliveryMethod)
                .OrderByDescending(d => d.Id)
                .ToList();
            ViewBag.Orders = new SelectList(_context.Orders.Select(o => new { o.Id }), "Id", "Id");
            ViewBag.Methods = new SelectList(_context.DeliveryMethods.ToList(), "Id", "Name");
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDelivery(int? id, int orderId, string address, int? deliveryMethodId, string status)
        {
            if (id.HasValue)
            {
                var d = _context.Deliveries.Find(id.Value);
                if (d != null)
                {
                    d.OrderId = orderId; d.Address = address; d.DeliveryMethodId = deliveryMethodId; d.Status = status;
                }
            }
            else
            {
                _context.Deliveries.Add(new DeliveryDb
                {
                    OrderId = orderId,
                    Address = address,
                    DeliveryMethodId = deliveryMethodId,
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                });
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Deliveries));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDelivery(int id)
        {
            var d = _context.Deliveries.Find(id);
            if (d != null) { _context.Deliveries.Remove(d); _context.SaveChanges(); }
            return RedirectToAction(nameof(Deliveries));
        }

        #endregion

        #region Reviews, Wishlist, Carts

        public IActionResult Reviews()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Отзывы";
            var list = _context.Reviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReview(int id)
        {
            var r = _context.Reviews.Find(id);
            if (r != null) { _context.Reviews.Remove(r); _context.SaveChanges(); }
            return RedirectToAction(nameof(Reviews));
        }

        public IActionResult Wishlists()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Избранное";
            var list = _context.Wishlists
                .Include(w => w.Product)
                .OrderByDescending(w => w.CreatedAt)
                .ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteWishlist(int id)
        {
            var w = _context.Wishlists.Find(id);
            if (w != null) { _context.Wishlists.Remove(w); _context.SaveChanges(); }
            return RedirectToAction(nameof(Wishlists));
        }

        public IActionResult Carts()
        {
            ViewData["AdminLayout"] = true;
            ViewData["Title"] = "Корзины";
            var list = _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.Items)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToList();
            return View(list);
        }

        #endregion

        private void LoadProductLookups()
        {
            ViewBag.Categories = new SelectList(_context.CategoriesDb.ToList(), "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brands.ToList(), "Id", "Name");
        }
    }
}
