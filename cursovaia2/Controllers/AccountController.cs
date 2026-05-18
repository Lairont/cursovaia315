using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            ViewData["FullWidth"] = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            ViewData["FullWidth"] = true;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email и пароль обязательны";
                return View();
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user != null && PasswordHasher.Verify(password, user.PasswordHash))
                {
                    AuthSession.SetUser(HttpContext.Session, user);

                    if (AuthSession.IsAdmin(HttpContext.Session))
                        return RedirectToAction("Index", "Admin");

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Неверный email или пароль";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ошибка при входе: " + ex.Message;
                return View();
            }
        }

        public IActionResult Register()
        {
            ViewData["FullWidth"] = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string passwordConfirm)
        {
            ViewData["FullWidth"] = true;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Все поля обязательны";
                return View();
            }

            if (password != passwordConfirm)
            {
                ViewBag.Error = "Пароли не совпадают";
                return View();
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    ViewBag.Error = "Пользователь с таким email уже существует";
                    return View();
                }

                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == AuthSession.UserRoleName);
                if (userRole == null)
                {
                    userRole = new RoleDb { Name = AuthSession.UserRoleName };
                    _context.Roles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                var newUser = new UserDb
                {
                    Email = email.Trim(),
                    PasswordHash = PasswordHasher.Hash(password),
                    RoleId = userRole.Id,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _context.Customers.Add(new CustomerDb
                {
                    UserId = newUser.Id,
                    Name = email.Split('@')[0],
                    BonusBalance = 0
                });
                await _context.SaveChangesAsync();

                newUser.Role = userRole;
                AuthSession.SetUser(HttpContext.Session, newUser);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ошибка при регистрации: " + ex.Message;
                return View();
            }
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction(nameof(Login));

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            var customer = _context.Customers.FirstOrDefault(c => c.UserId == user.Id);

            var model = new ProfileViewModel
            {
                Email = user.Email,
                Name = customer?.Name ?? user.Email,
                Phone = customer?.Phone,
                RoleName = user.Role?.Name ?? "user",
                RegisteredAt = user.CreatedAt,
                BonusBalance = customer?.BonusBalance ?? 0
            };

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
