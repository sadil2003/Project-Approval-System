using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;

namespace PAS_BlindMatching.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // View all users
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            var users = await _context.Users.ToListAsync();
            return View("~/Views/user/Users.cshtml", users);
        }

        // View all projects (main dashboard)
        public async Task<IActionResult> Projects()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var projects = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechStack = p.TechStack,
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    SupervisorName = p.Supervisor != null ? p.Supervisor.Name : "Not Assigned",
                    StudentName = p.Student != null ? p.Student.Name : "Unknown",
                    StudentEmail = p.Student != null ? p.Student.Email : "Unknown"
                })
                .ToListAsync();

            return View(projects);
        }

        // Reassign supervisor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignSupervisor(int projectId, int newSupervisorId)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            var supervisorExists = await _context.Users
                .AnyAsync(u => u.Id == newSupervisorId && u.Role == "Supervisor");

            if (!supervisorExists)
            {
                TempData["Error"] = "Supervisor ID not found or user is not a Supervisor.";
                return RedirectToAction("Projects");
            }

            project.SupervisorId = newSupervisorId;
            project.Status = "Matched";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Supervisor reassigned successfully!";
            return RedirectToAction("Projects");
        }

        // Delete a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }
    }
}
