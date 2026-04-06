using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;

namespace PAS_BlindMatching.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly AppDbContext _context;

        public SupervisorController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Show available projects (Blind Feed)
        public async Task<IActionResult> Index()
        {
            // Ensure only authorized supervisors can access
            var role = HttpContext.Session.GetString("Role");
            if (role != "Supervisor") return RedirectToAction("Login", "Account");

            var projects = await _context.Projects
                .Where(p => p.Status == "Pending")
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,   // Needed for the Modal popup
                    TechStack = p.TechStack, // Needed for the Modal popup
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    StudentName = "Hidden (Blind Review)"
                }).ToListAsync();

            return View(projects);
        }

        // 2. Handle the Confirm Match button
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectProject(int id)
        {
            var supervisorId = HttpContext.Session.GetInt32("UserId");
            if (supervisorId == null) return RedirectToAction("Login", "Account");

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            // Update project status and assign current supervisor
            project.SupervisorId = supervisorId;
            project.Status = "Matched";

            await _context.SaveChangesAsync();

            // Redirect to MyMatches to see the revealed student info
            return RedirectToAction("MyMatches");
        }

        // 3. Show confirmed matches (REVEALED identity)
        public async Task<IActionResult> MyMatches()
        {
            var supervisorId = HttpContext.Session.GetInt32("UserId");
            if (supervisorId == null) return RedirectToAction("Login", "Account");

            var matchedProjects = await _context.Projects
                .Where(p => p.SupervisorId == supervisorId)
                .Include(p => p.Student) // Joins User table for Name/Email
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,   // Passed to View for the Modal
                    TechStack = p.TechStack, // Passed to View for the Modal
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    // Identity is now revealed
                    StudentName = p.Student != null ? p.Student.Name : "Unknown",
                    StudentEmail = p.Student != null ? p.Student.Email : "Unknown"
                })
                .ToListAsync();

            return View(matchedProjects);
        }
    }
}