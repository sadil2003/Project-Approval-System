using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;
using System.Text.Json;

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

        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var users = await _context.Users.ToListAsync();
            return View("~/Views/user/Users.cshtml", users);
        }

        public async Task<IActionResult> Projects()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var projectsData = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .ToListAsync();

            var projects = projectsData.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechStack = p.TechStack,
                ResearchArea = p.ResearchArea,
                Status = p.Status,
                SupervisorName = p.Supervisor != null ? p.Supervisor.Name : "Not Assigned",
                StudentName = p.Student != null ? p.Student.Name : "Unknown",
                StudentEmail = p.Student != null ? p.Student.Email : "Unknown",
                GroupMembers = !string.IsNullOrEmpty(p.GroupMembersJson)
                    ? JsonSerializer.Deserialize<List<GroupMemberInfo>>(p.GroupMembersJson)
                    : null
            }).ToList();

            return View(projects);
        }

        public async Task<IActionResult> Pairings()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var pairedData = await _context.Projects
                .Where(p => p.Status == "Matched" && p.SupervisorId != null)
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .ToListAsync();

            var paired = pairedData.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Title = p.Title,
                ResearchArea = p.ResearchArea,
                TechStack = p.TechStack,
                Abstract = p.Abstract,
                Status = p.Status,
                StudentName = p.Student != null ? p.Student.Name : "Unknown",
                StudentEmail = p.Student != null ? p.Student.Email : "Unknown",
                SupervisorName = p.Supervisor != null ? p.Supervisor.Name : "Unknown",
                GroupMembers = !string.IsNullOrEmpty(p.GroupMembersJson)
                    ? JsonSerializer.Deserialize<List<GroupMemberInfo>>(p.GroupMembersJson)
                    : null
            }).ToList();

            return View(paired);
        }

        public async Task<IActionResult> EditUser(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            ViewBag.ResearchAreas = ResearchAreaList.Areas;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int userId, string name, string email, string role, string? researchArea)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != userId);

            if (emailTaken)
            {
                TempData["Error"] = "That email is already used by another user.";
                return RedirectToAction("Users");
            }

            user.Name = name;
            user.Email = email;
            user.Role = role;
            user.ResearchArea = string.IsNullOrWhiteSpace(researchArea) ? null : researchArea;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"User '{name}' updated successfully.";

            return RedirectToAction("Users");
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var userProjects = await _context.Projects
                .Where(p => p.StudentId == userId)
                .ToListAsync();

            if (userProjects.Any())
                _context.Projects.RemoveRange(userProjects);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User '{user.Name}' and their {userProjects.Count} project(s) were deleted.";

            return RedirectToAction("Users");
        }
    }
}