using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    [Authorize(Roles = "ModuleLeader")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Students = await _context.Students.ToListAsync();
            ViewBag.Supervisors = await _context.Supervisors.ToListAsync();

            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalSupervisors = await _context.Supervisors.CountAsync();
            ViewBag.PendingProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Pending);
            ViewBag.MatchedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Matched);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(string name, string email)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
            {
                if (!await _context.Students.AnyAsync(s => s.Email == email) &&
                    !await _context.Supervisors.AnyAsync(s => s.Email == email))
                {
                    _context.Students.Add(new Student { Name = name, Email = email });
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Error"] = "This Email is already registered!";
                }
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> AddSupervisor(string name, string email)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
            {
                if (!await _context.Students.AnyAsync(s => s.Email == email) &&
                    !await _context.Supervisors.AnyAsync(s => s.Email == email))
                {
                    _context.Supervisors.Add(new Supervisor { 
                        Name = name, 
                        Email = email, 
                        Bio = "Not Specified" 
                    });
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Error"] = "This Email is already registered!";
                }
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(int id, string name, string email)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
            {
                bool emailExists = await _context.Students.AnyAsync(s => s.Email == email && s.StudentId != id) ||
                                   await _context.Supervisors.AnyAsync(s => s.Email == email);
                if (!emailExists)
                {
                    student.Name = name;
                    student.Email = email;
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Error"] = "This Email is already registered to another user!";
                }
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> EditSupervisor(int id, string name, string email)
        {
            var supervisor = await _context.Supervisors.FindAsync(id);
            if (supervisor != null && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
            {
                bool emailExists = await _context.Supervisors.AnyAsync(s => s.Email == email && s.SupervisorId != id) ||
                                   await _context.Students.AnyAsync(s => s.Email == email);
                if (!emailExists)
                {
                    supervisor.Name = name;
                    supervisor.Email = email;
                    _context.Update(supervisor);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Error"] = "This Email is already registered to another user!";
                }
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                var linkedProjects = await _context.Projects.Where(p => p.StudentId == id).ToListAsync();
                if (linkedProjects.Any())
                {
                    _context.Projects.RemoveRange(linkedProjects);
                }

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student deleted successfully!";
            }
            return RedirectToAction("Dashboard");
        }

     
        [HttpPost]
        public async Task<IActionResult> DeleteSupervisor(int id)
        {
            var supervisor = await _context.Supervisors.FindAsync(id);
            if (supervisor != null)
            {
                
                var linkedProjects = await _context.Projects.Where(p => p.SupervisorId == id).ToListAsync();

                foreach (var project in linkedProjects)
                {
                    project.SupervisorId = null; 
                    project.Status = ProjectStatus.Pending; 
                    project.CurrentMilestone = "Supervisor Removed. Waiting for new allocation.";
                }

              
                _context.Supervisors.Remove(supervisor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Supervisor deleted successfully!";
            }
            return RedirectToAction("Dashboard");
        }

        // ==========================================
        // ALLOCATION OVERSIGHT
        // ==========================================
        public async Task<IActionResult> Allocations()
        {
            var matches = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Where(p => p.Status == ProjectStatus.Matched)
                .ToListAsync();

            return View(matches);
        }

        public async Task<IActionResult> ResearchAreas()
        {
            var areas = await _context.ResearchAreas.ToListAsync();
            return View(areas);
        }

        [HttpPost]
        public async Task<IActionResult> AddResearchArea(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.ResearchAreas.Add(new ResearchArea { Name = name });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research area added successfully!";
            }
            return RedirectToAction("ResearchAreas");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResearchArea(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null)
            {
                _context.ResearchAreas.Remove(area);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research area deleted successfully!";
            }
            return RedirectToAction("ResearchAreas");
        }
    }
}
