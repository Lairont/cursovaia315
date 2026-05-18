using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace cursovaia2.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PricingService _pricing;

        public CheckoutController(ApplicationDbContext context, PricingService pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            ReferenceDataSeeder.EnsureDeliveryAndPaymentMethods(_context);

            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
            if (customer == null)
                return RedirectToAction("Index", "Cart");

            var dbCart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerId == customer.Id);

            if (dbCart == null || !dbCart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var cart = new Cart();
            foreach (var item in dbCart.Items)
            {
                if (item.Product == null) continue;
                var price = _pricing.GetProductPrice(item.Product.Id, item.Product.Price).FinalPrice;
                cart.AddItem(new Product
                {
                    Id = item.Product.Id,
                    Name = item.Product.Name,
                    Price = price
                }, item.Quantity);
            }

            var deliveryMethods = _context.DeliveryMethods.OrderBy(m => m.Price).ToList();
            var paymentMethods = _context.PaymentMethods.OrderBy(m => m.Name).ToList();

            var model = new CheckoutViewModel
            {
                Cart = cart,
                Subtotal = cart.Total,
                DeliveryMethods = deliveryMethods,
                PaymentMethods = paymentMethods
            };

            ViewBag.Customer = customer;
            return View(model);
        }

        [HttpPost]
        public IActionResult ApplyPromo([FromBody] PromoRequest request)
        {
            var subtotal = request.Subtotal;
            if (subtotal <= 0 && request.CartItems != null)
                subtotal = request.CartItems.Sum(i => i.Price * i.Quantity);

            var result = _pricing.ValidatePromoCode(request.PromoCode, subtotal);
            if (result == null || !result.Success)
                return Json(new { success = false, message = result?.Message ?? "Введите промокод" });

            return Json(new
            {
                success = true,
                message = result.Message,
                discountAmount = result.DiscountAmount,
                newTotal = result.NewTotal
            });
        }

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] CheckoutModel? model)
        {
            if (model == null)
                return Json(new { success = false, message = "Некорректные данные заказа" });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                    return Json(new { success = false, message = "Не авторизован" });

                if (string.IsNullOrWhiteSpace(model.Address) || string.IsNullOrWhiteSpace(model.City))
                    return Json(new { success = false, message = "Укажите адрес и город доставки" });

                if (!model.DeliveryMethodId.HasValue)
                    return Json(new { success = false, message = "Выберите способ доставки" });

                if (!model.PaymentMethodId.HasValue)
                    return Json(new { success = false, message = "Выберите способ оплаты" });

                var deliveryMethod = _context.DeliveryMethods.Find(model.DeliveryMethodId.Value);
                if (deliveryMethod == null)
                    return Json(new { success = false, message = "Способ доставки не найден" });

                var paymentMethod = _context.PaymentMethods.Find(model.PaymentMethodId.Value);
                if (paymentMethod == null)
                    return Json(new { success = false, message = "Способ оплаты не найден" });

                var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
                if (customer == null)
                    return Json(new { success = false, message = "Профиль покупателя не найден" });

                var dbCart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.CustomerId == customer.Id);

                if (dbCart == null || !dbCart.Items.Any())
                    return Json(new { success = false, message = "Корзина пуста" });

                decimal subtotal = 0;
                var orderLines = new List<(int ProductId, int Quantity, decimal Price)>();

                foreach (var item in dbCart.Items)
                {
                    if (item.Product == null) continue;
                    var price = _pricing.GetProductPrice(item.Product.Id, item.Product.Price).FinalPrice;
                    orderLines.Add((item.ProductId, item.Quantity, price));
                    subtotal += price * item.Quantity;
                }

                if (!orderLines.Any())
                    return Json(new { success = false, message = "Нет товаров для заказа" });

                var deliveryPrice = deliveryMethod.Price;
                var totalBeforePromo = subtotal + deliveryPrice;

                int? promoCodeId = null;
                int? discountId = null;
                decimal promoDiscount = 0;

                if (!string.IsNullOrWhiteSpace(model.PromoCode))
                {
                    var promoResult = _pricing.ValidatePromoCode(model.PromoCode, totalBeforePromo);
                    if (promoResult == null || !promoResult.Success)
                        return Json(new { success = false, message = promoResult?.Message ?? "Неверный промокод" });

                    promoCodeId = promoResult.PromoCodeId;
                    discountId = promoResult.DiscountId;
                    promoDiscount = promoResult.DiscountAmount;
                }

                var finalPrice = Math.Max(0, totalBeforePromo - promoDiscount);

                var order = new OrderDb
                {
                    CustomerId = customer.Id,
                    Status = "pending",
                    TotalPrice = subtotal,
                    FinalPrice = finalPrice,
                    DiscountId = discountId,
                    PromoCodeId = promoCodeId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var line in orderLines)
                {
                    _context.OrderItems.Add(new OrderItemDb
                    {
                        OrderId = order.Id,
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        Price = line.Price
                    });
                }

                if (promoCodeId.HasValue)
                {
                    var promo = _context.PromoCodes.Find(promoCodeId.Value);
                    if (promo != null) promo.TimesUsed++;
                }

                _context.OrderStatusHistories.Add(new OrderStatusHistoryDb
                {
                    OrderId = order.Id,
                    OldStatus = null,
                    NewStatus = "pending",
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Comment = string.IsNullOrWhiteSpace(model.PromoCode)
                        ? "Заказ создан"
                        : $"Заказ создан. Промокод: {model.PromoCode}"
                });

                var fullAddress = $"{model.City.Trim()}, {model.Address.Trim()}" +
                    (string.IsNullOrWhiteSpace(model.PostalCode) ? "" : $", {model.PostalCode.Trim()}");

                _context.Deliveries.Add(new DeliveryDb
                {
                    OrderId = order.Id,
                    Address = fullAddress,
                    DeliveryMethodId = deliveryMethod.Id,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                });

                _context.Payments.Add(new PaymentDb
                {
                    OrderId = order.Id,
                    Amount = finalPrice,
                    MethodId = paymentMethod.Id,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                });

                if (!string.IsNullOrWhiteSpace(model.Phone))
                    customer.Phone = model.Phone.Trim();

                _context.CartItems.RemoveRange(dbCart.Items);
                dbCart.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, orderId = order.Id, message = "Заказ успешно оформлен" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                var detail = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"Ошибка оформления: {detail}" });
            }
        }

        public IActionResult OrderConfirmed(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var customer = userId.HasValue
                ? _context.Customers.FirstOrDefault(c => c.UserId == userId.Value)
                : null;

            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == orderId && (customer == null || o.CustomerId == customer.Id));

            if (order == null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }

        public IActionResult Orders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
            if (customer == null)
                return RedirectToAction("Index", "Home");

            var orders = _context.Orders
                .Where(o => o.CustomerId == customer.Id)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }
    }

    public class CheckoutModel
    {
        [JsonPropertyName("address")]
        public string Address { get; set; } = "";

        [JsonPropertyName("city")]
        public string City { get; set; } = "";

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = "";

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = "";

        [JsonPropertyName("promoCode")]
        public string? PromoCode { get; set; }

        [JsonPropertyName("deliveryMethodId")]
        public int? DeliveryMethodId { get; set; }

        [JsonPropertyName("paymentMethodId")]
        public int? PaymentMethodId { get; set; }
    }

    public class PromoRequest
    {
        public string? PromoCode { get; set; }
        public decimal Subtotal { get; set; }
        public List<PromoCartItem>? CartItems { get; set; }
    }

    public class PromoCartItem
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
