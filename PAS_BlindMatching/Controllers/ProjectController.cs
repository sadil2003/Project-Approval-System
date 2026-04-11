using Microsoft.AspNetCore.Mvc;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PAS_BlindMatching.Controllers
{
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Submit()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student" && role != "Group") return Unauthorized();

            ViewBag.ResearchAreas = ResearchAreaList.Areas;
            ViewBag.IsGroup = role == "Group";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            string title,
            string @abstract,
            string techStack,
            string? researchArea,
            List<string>? memberNames,
            List<string>? memberIds)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            ViewBag.ResearchAreas = ResearchAreaList.Areas;
            ViewBag.IsGroup = role == "Group";

            string? groupMembersJson = null;

            if (role == "Group")
            {
                var names = memberNames?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
                var ids = memberIds?.Where(i => !string.IsNullOrWhiteSpace(i)).ToList() ?? new List<string>();

                if (names.Count == 0)
                {
                    ModelState.AddModelError("", "Group submissions must include at least one member.");
                    return View();
                }

                if (names.Count != ids.Count)
                {
                    ModelState.AddModelError("", "Each member must have both name and ID.");
                    return View();
                }

                var members = names.Zip(ids, (n, i) => new GroupMemberInfo
                {
                    Name = n,
                    UserId = i
                }).ToList();

                groupMembersJson = JsonSerializer.Serialize(members);
            }

            var project = new Project
            {
                Title = title,
                Abstract = @abstract,
                TechStack = techStack,
                ResearchArea = string.IsNullOrWhiteSpace(researchArea) ? "Not Specified" : researchArea,
                StudentId = userId.Value,
                Status = "Pending",
                GroupMembersJson = groupMembersJson
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyProjects");
        }

        public async Task<IActionResult> MyProjects()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            // ✅ FIRST get data from DB
            var data = await _context.Projects
                .Where(p => p.StudentId == userId.Value)
                .Include(p => p.Supervisor)
                .Include(p => p.Student)
                .ToListAsync();

            // ✅ THEN process safely
            var projects = data.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechStack = p.TechStack,
                ResearchArea = p.ResearchArea,
                Status = p.Status,
                SupervisorName = p.Supervisor != null ? p.Supervisor.Name : null,
                StudentName = p.Student.Name,
                StudentEmail = p.Student.Email,
                GroupMembers = !string.IsNullOrEmpty(p.GroupMembersJson)
                    ? JsonSerializer.Deserialize<List<GroupMemberInfo>>(p.GroupMembersJson)
                    : null
            }).ToList();

            return View(projects);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId.Value);

            if (project == null) return NotFound();

            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only edit Pending projects.";
                return RedirectToAction("MyProjects");
            }

            ViewBag.ResearchAreas = ResearchAreaList.Areas;
            ViewBag.IsGroup = role == "Group";

            var vm = new ProjectViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Abstract = project.Abstract,
                TechStack = project.TechStack,
                ResearchArea = project.ResearchArea,
                Status = project.Status,
                GroupMembers = !string.IsNullOrEmpty(project.GroupMembersJson)
                    ? JsonSerializer.Deserialize<List<GroupMemberInfo>>(project.GroupMembersJson)
                    : null
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string title,
            string @abstract,
            string techStack,
            string? researchArea,
            List<string>? memberNames,
            List<string>? memberIds)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if ((role != "Student" && role != "Group") || userId == null) return Unauthorized();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId.Value);

            if (project == null) return NotFound();

            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only edit Pending projects.";
                return RedirectToAction("MyProjects");
            }

            string? groupMembersJson = null;

            if (role == "Group")
            {
                var names = memberNames?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
                var ids = memberIds?.Where(i => !string.IsNullOrWhiteSpace(i)).ToList() ?? new List<string>();

                if (names.Count == 0 || names.Count != ids.Count)
                {
                    TempData["Error"] = "Invalid group members.";
                    return RedirectToAction("Edit", new { id });
                }

                var members = names.Zip(ids, (n, i) => new GroupMemberInfo
                {
                    Name = n,
                    UserId = i
                }).ToList();

                groupMembersJson = JsonSerializer.Serialize(members);
            }

            project.Title = title;
            project.Abstract = @abstract;
            project.TechStack = techStack;
            project.ResearchArea = string.IsNullOrWhiteSpace(researchArea) ? "Not Specified" : researchArea;
            project.GroupMembersJson = groupMembersJson;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Project updated successfully.";
            return RedirectToAction("MyProjects");
        }
    }
}