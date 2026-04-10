using Microsoft.AspNetCore.Mvc;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;
using Microsoft.EntityFrameworkCore;

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
            if (role != "Student" && role != "Group") return Unauthorized();

            ViewBag.ResearchAreas = ResearchAreaList.Areas;
            return View();
        }

        // POST: /Project/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string title, string @abstract, string techStack, string? researchArea)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            ViewBag.ResearchAreas = ResearchAreaList.Areas;

            var project = new Project
            {
                Title = title,
                Abstract = @abstract,
                TechStack = techStack,
                ResearchArea = string.IsNullOrWhiteSpace(researchArea) ? "Not Specified" : researchArea,
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

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            var projects = await _context.Projects
                .Where(p => p.StudentId == userId.Value)
                .Include(p => p.Supervisor)
                .Include(p => p.Student)
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

            return View(projects);
        }

        // GET: /Project/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId.Value);

            if (project == null) return NotFound();

            // Only allow editing if not yet matched
            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only edit proposals that are still Pending.";
                return RedirectToAction("MyProjects");
            }

            ViewBag.ResearchAreas = ResearchAreaList.Areas;

            var vm = new ProjectViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Abstract = project.Abstract,
                TechStack = project.TechStack,
                ResearchArea = project.ResearchArea,
                Status = project.Status
            };

            return View(vm);
        }

        // POST: /Project/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string @abstract, string techStack, string? researchArea)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            ViewBag.ResearchAreas = ResearchAreaList.Areas;

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId.Value);

            if (project == null) return NotFound();

            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only edit proposals that are still Pending.";
                return RedirectToAction("MyProjects");
            }

            project.Title = title;
            project.Abstract = @abstract;
            project.TechStack = techStack;
            project.ResearchArea = string.IsNullOrWhiteSpace(researchArea) ? "Not Specified" : researchArea;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proposal updated successfully.";
            return RedirectToAction("MyProjects");
        }
    }
}
