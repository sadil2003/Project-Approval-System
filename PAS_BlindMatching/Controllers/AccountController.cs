using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;

namespace PAS_BlindMatching.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            ViewBag.ResearchAreas = ResearchAreaList.Areas;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password, string role, string? researchArea)
        {
            ViewBag.ResearchAreas = ResearchAreaList.Areas;

            // Validate role
            var validRoles = new[] { "Student", "Group", "Supervisor" };
            if (string.IsNullOrWhiteSpace(role) || !validRoles.Contains(role))
            {
                ModelState.AddModelError("", "Please select a valid role.");
                return View();
            }

            // Supervisors MUST provide a research area
            if (role == "Supervisor" && string.IsNullOrWhiteSpace(researchArea))
            {
                ModelState.AddModelError("", "Research area is mandatory for Supervisors.");
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            var user = new User
            {
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                ResearchArea = string.IsNullOrWhiteSpace(researchArea) ? null : researchArea
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);

            return RedirectUserByRole(user.Role);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectUserByRole(user.Role);
        }

        // GET: /Account/AdminLogin
        public IActionResult AdminLogin()
        {
            return View();
        }

        // POST: /Account/AdminLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(string username, string password)
        {
            // Admin login uses Name + Password (not email)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == username && u.Password == password && u.Role == "Admin");

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid admin username or password.");
                return View();
            }

            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Users", "Admin");
        }

        private IActionResult RedirectUserByRole(string role)
        {
            if (role == "Student" || role == "Group")
                return RedirectToAction("Submit", "Project");
            else if (role == "Supervisor")
                return RedirectToAction("Index", "Supervisor");
            else if (role == "Admin")
                return RedirectToAction("Users", "Admin");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
