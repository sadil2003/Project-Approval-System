using Microsoft.AspNetCore.Mvc;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data; // This tells the code where to find AppDbContext

namespace PAS_BlindMatching.Controllers
{
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Project/Submit
        public IActionResult Submit()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student") return Unauthorized();

            return View();
        }

        // POST: /Project/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string title, string @abstract, string techStack, string researchArea)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (role != "Student" || userId == null) return Unauthorized();

            var project = new Project
            {
                Title = title,
                Abstract = @abstract,
                TechStack = techStack,
                ResearchArea = researchArea,
                StudentId = userId.Value,
                Status = "Pending"
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyProjects");
        }

        // GET: /Project/MyProjects
        public async Task<IActionResult> MyProjects()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (role != "Student" || userId == null) return Unauthorized();

            // Map Project to ProjectViewModel
            var projects = await _context.Projects
                .Where(p => p.StudentId == userId.Value)
                .Include(p => p.Supervisor)  // for supervisor name
                .Include(p => p.Student)     // for student name/email
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechStack = p.TechStack,
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    SupervisorName = p.Supervisor != null ? p.Supervisor.Name : null,
                    StudentName = p.Student.Name,
                    StudentEmail = p.Student.Email
                })
                .ToListAsync();

            return View(projects); // ✅ this matches your @model List<ProjectViewModel>
        }
    }
}