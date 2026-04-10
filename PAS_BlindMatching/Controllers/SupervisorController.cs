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

        // 1. Show available projects filtered by supervisor's research area (Blind Feed)
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var supervisorId = HttpContext.Session.GetInt32("UserId");

            if (role != "Supervisor") return RedirectToAction("Login", "Account");

            // Get this supervisor's research area from DB
            var supervisor = await _context.Users.FindAsync(supervisorId);
            var supervisorArea = supervisor?.ResearchArea;

            // Filter projects: Pending AND matching supervisor's research area (if they have one)
            var query = _context.Projects
                .Where(p => p.Status == "Pending");

            if (!string.IsNullOrWhiteSpace(supervisorArea))
                query = query.Where(p => p.ResearchArea == supervisorArea);

            var projects = await query
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechStack = p.TechStack,
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    StudentName = "Hidden (Blind Review)"
                }).ToListAsync();

            ViewBag.SupervisorArea = supervisorArea;
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

            project.SupervisorId = supervisorId;
            project.Status = "Matched";

            await _context.SaveChangesAsync();
            return RedirectToAction("MyMatches");
        }

        // 3. Show confirmed matches (REVEALED identity)
        public async Task<IActionResult> MyMatches()
        {
            var supervisorId = HttpContext.Session.GetInt32("UserId");
            if (supervisorId == null) return RedirectToAction("Login", "Account");

            var matchedProjects = await _context.Projects
                .Where(p => p.SupervisorId == supervisorId)
                .Include(p => p.Student)
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechStack = p.TechStack,
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    StudentName = p.Student != null ? p.Student.Name : "Unknown",
                    StudentEmail = p.Student != null ? p.Student.Email : "Unknown"
                })
                .ToListAsync();

            return View(matchedProjects);
        }
    }
}
