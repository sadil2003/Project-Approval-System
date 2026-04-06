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

        // View all users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // View all projects (The main dashboard)
        public async Task<IActionResult> Projects()
        {
            var projects = await _context.Projects
                .Include(p => p.Student)      // Join Student table
                .Include(p => p.Supervisor)   // Join Supervisor table
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

        // Reassign supervisor - FIX: Ensures the correct redirection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignSupervisor(int projectId, int newSupervisorId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            // Verify the new supervisor actually exists
            var supervisorExists = await _context.Users.AnyAsync(u => u.Id == newSupervisorId && u.Role == "Supervisor");
            if (!supervisorExists)
            {
                TempData["Error"] = "Supervisor ID not found or user is not a Supervisor.";
                return RedirectToAction("Projects");
            }

            project.SupervisorId = newSupervisorId;
            project.Status = "Matched"; // Update status since it's now assigned

            await _context.SaveChangesAsync();
            TempData["Success"] = "Supervisor reassigned successfully!";

            return RedirectToAction("Projects");
        }

        // Delete a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }
    }
}