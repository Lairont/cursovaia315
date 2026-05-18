using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PricingService _pricing;

        public CartController(ApplicationDbContext context, PricingService pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        public IActionResult Index()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    return View(GetSessionCart());
                }

                var customer = GetOrCreateCustomer(userId.Value);
                var dbCart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.CustomerId == customer.Id);

                if (dbCart == null)
                {
                    dbCart = new CartDb { CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
                    _context.Carts.Add(dbCart);
                    _context.SaveChanges();
                }

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

                return View(cart);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при загрузке корзины: {ex.Message}";
                return View(new Cart());
            }
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartRequest? request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(new { success = false, message = "Некорректный запрос" });

            try
            {
                var quantity = request.Quantity > 0 ? request.Quantity : 1;
                var product = _context.ProductsDb.FirstOrDefault(p => p.Id == request.ProductId && p.IsActive);

                if (product == null)
                    return Json(new { success = false, message = "Товар не найден" });

                var userId = HttpContext.Session.GetInt32("UserId");

                var effectivePrice = _pricing.GetProductPrice(product.Id, product.Price).FinalPrice;

                if (!userId.HasValue)
                {
                    var sessionCart = GetSessionCart();
                    sessionCart.AddItem(new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = effectivePrice
                    }, quantity);
                    SaveSessionCart(sessionCart);

                    return Json(new { success = true, message = "Товар добавлен в корзину", cartCount = sessionCart.ItemCount });
                }

                var customer = GetOrCreateCustomer(userId.Value);
                var dbCart = _context.Carts.FirstOrDefault(c => c.CustomerId == customer.Id);
                if (dbCart == null)
                {
                    dbCart = new CartDb { CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
                    _context.Carts.Add(dbCart);
                    _context.SaveChanges();
                }

                var cartItem = _context.CartItems.FirstOrDefault(ci => ci.CartId == dbCart.Id && ci.ProductId == request.ProductId);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    cartItem.Price = effectivePrice;
                }
                else
                {
                    _context.CartItems.Add(new CartItemDb
                    {
                        CartId = dbCart.Id,
                        ProductId = request.ProductId,
                        Quantity = quantity,
                        Price = effectivePrice
                    });
                }

                dbCart.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                var cartCount = _context.CartItems.Where(ci => ci.CartId == dbCart.Id).Sum(ci => ci.Quantity);
                return Json(new { success = true, message = "Товар добавлен в корзину", cartCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult RemoveFromCart([FromBody] CartRequest? request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(new { success = false, message = "Некорректный запрос" });

            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    var cart = GetSessionCart();
                    cart.RemoveItem(request.ProductId);
                    SaveSessionCart(cart);
                    return Json(new { success = true, cartCount = cart.ItemCount });
                }

                var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
                if (customer == null)
                    return Json(new { success = false, message = "Покупатель не найден" });

                var dbCart = _context.Carts.FirstOrDefault(c => c.CustomerId == customer.Id);
                if (dbCart == null)
                    return Json(new { success = false, message = "Корзина не найдена" });

                var cartItem = _context.CartItems.FirstOrDefault(ci => ci.CartId == dbCart.Id && ci.ProductId == request.ProductId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    _context.SaveChanges();
                }

                var cartCount = _context.CartItems.Where(ci => ci.CartId == dbCart.Id).Sum(ci => (int?)ci.Quantity) ?? 0;
                return Json(new { success = true, cartCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] CartRequest? request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(new { success = false, message = "Некорректный запрос" });

            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    var cart = GetSessionCart();
                    if (request.Quantity <= 0)
                        cart.RemoveItem(request.ProductId);
                    else
                        cart.UpdateQuantityByProductId(request.ProductId, request.Quantity);
                    SaveSessionCart(cart);
                    return Json(new { success = true, cartCount = cart.ItemCount });
                }

                var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
                if (customer == null)
                    return Json(new { success = false, message = "Покупатель не найден" });

                var dbCart = _context.Carts.FirstOrDefault(c => c.CustomerId == customer.Id);
                if (dbCart == null)
                    return Json(new { success = false, message = "Корзина не найдена" });

                var cartItem = _context.CartItems.FirstOrDefault(ci => ci.CartId == dbCart.Id && ci.ProductId == request.ProductId);
                if (cartItem == null)
                    return Json(new { success = false, message = "Товар не найден в корзине" });

                if (request.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = request.Quantity;
                }

                dbCart.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                var cartCount = _context.CartItems.Where(ci => ci.CartId == dbCart.Id).Sum(ci => (int?)ci.Quantity) ?? 0;
                return Json(new { success = true, cartCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    SaveSessionCart(new Cart());
                    return Json(new { success = true });
                }

                var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
                if (customer == null)
                    return Json(new { success = false });

                var dbCart = _context.Carts.FirstOrDefault(c => c.CustomerId == customer.Id);
                if (dbCart != null)
                {
                    var items = _context.CartItems.Where(ci => ci.CartId == dbCart.Id).ToList();
                    _context.CartItems.RemoveRange(items);
                    _context.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            return Json(new { count = GetCartItemCount() });
        }

        [HttpGet]
        public IActionResult GetCartData()
        {
            return Json(new { itemCount = GetCartItemCount() });
        }

        private int GetCartItemCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
                return GetSessionCart().ItemCount;

            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId.Value);
            if (customer == null)
                return 0;

            var dbCart = _context.Carts.FirstOrDefault(c => c.CustomerId == customer.Id);
            if (dbCart == null)
                return 0;

            return _context.CartItems.Where(ci => ci.CartId == dbCart.Id).Sum(ci => ci.Quantity);
        }

        private CustomerDb GetOrCreateCustomer(int userId)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);
            if (customer != null)
                return customer;

            customer = new CustomerDb
            {
                UserId = userId,
                Name = HttpContext.Session.GetString("Email") ?? "Пользователь",
                BonusBalance = 0
            };
            _context.Customers.Add(customer);
            _context.SaveChanges();
            return customer;
        }

        private Cart GetSessionCart()
        {
            return HttpContext.Session.Get<Cart>("cart") ?? new Cart();
        }

        private void SaveSessionCart(Cart cart)
        {
            HttpContext.Session.Set("cart", cart);
        }
    }
}
