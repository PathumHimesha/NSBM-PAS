using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Models;
using PAS_Project.Services; 
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;

        public ProjectsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IEmailService emailService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }

        
        public async Task<IActionResult> Index()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (userRole == "Student" && int.TryParse(userIdClaim, out int studentId))
            {
                var projects = await _context.Projects.Include(p => p.Student).Include(p => p.Supervisor)
                    .Where(p => p.StudentId == studentId).ToListAsync();
                return View(projects);
            }
            else if (userRole == "Supervisor")
            {
                var projects = await _context.Projects.Include(p => p.Student).Include(p => p.Supervisor)
                    .Where(p => p.Status == ProjectStatus.Pending)
                    .ToListAsync();
                return View(projects);
            }

            return View(await _context.Projects.Include(p => p.Student).Include(p => p.Supervisor).ToListAsync());
        }

        
        public async Task<IActionResult> MyAcceptedProjects()
        {
            var supervisorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (int.TryParse(supervisorIdClaim, out int supervisorId))
            {
                var myProjects = await _context.Projects
                    .Include(p => p.Student)
                    .Where(p => p.SupervisorId == supervisorId && p.Status == ProjectStatus.Matched)
                    .ToListAsync();
                return View(myProjects);
            }
            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> MatchDetails(int? id)
        {
            if (id == null) return NotFound();
            
            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(m => m.ProjectId == id);
                
            if (project == null) return NotFound();
            
            return View(project);
        }

       
        public async Task<IActionResult> Create()
        {
           
            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,Abstract,TechnicalStack,ResearchArea")] Project project, IFormFile? proposalFile)
        {
            if (ModelState.IsValid)
            {
                
                if (proposalFile != null && proposalFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(proposalFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await proposalFile.CopyToAsync(fileStream);
                    }
                    project.ProposalFilePath = "/uploads/" + uniqueFileName;
                }

               
                project.Status = ProjectStatus.Pending;
                project.ProgressPercentage = 10;
                project.CurrentMilestone = "Proposal Submitted";
                
                var studentIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (int.TryParse(studentIdClaim, out int studentId))
                {
                    project.StudentId = studentId;
                }

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
          
            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            return View(project);
        }

      
        [HttpPost]
        public async Task<IActionResult> ExpressInterest(int projectId)
        {
            var project = await _context.Projects.Include(p => p.Student).FirstOrDefaultAsync(p => p.ProjectId == projectId);
            if (project != null)
            {
                var supervisorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var supervisorName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; 

                if (int.TryParse(supervisorIdClaim, out int supervisorId))
                {
                    project.SupervisorId = supervisorId;
                    project.Status = ProjectStatus.Matched;
                    
                   
                    project.ProgressPercentage = 25;
                    project.CurrentMilestone = "Supervisor Assigned & Planning";
                    
                    _context.Update(project);
                    await _context.SaveChangesAsync();

                
                    if (project.Student != null && !string.IsNullOrEmpty(project.Student.Email))
                    {
                        string subject = "🎉 Project Proposal Accepted! - NSBM PAS";
                        string body = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px; max-width: 600px;'>
                                <h2 style='color: #2563eb;'>Congratulations {project.Student.Name}!</h2>
                                <p>Your Final Year Project proposal <strong>'{project.Title}'</strong> has been successfully accepted.</p>
                                <p><strong>Assigned Supervisor:</strong> {supervisorName}</p>
                                <p>Please log in to your NSBM PAS Dashboard to view the milestones and start your development journey.</p>
                                <br/>
                                <p style='color: #64748b; font-size: 12px;'>This is an automated message from the NSBM Project Allocation System.</p>
                            </div>";
                        
                        _ = _emailService.SendEmailAsync(project.Student.Email, subject, body);
                    }

                    return RedirectToAction(nameof(MyAcceptedProjects));
                }
            }
            return RedirectToAction(nameof(Index));
        }

       
        [HttpPost]
        public async Task<IActionResult> UpdateMilestone(int projectId, string milestone, int progress)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                
               
                if (userRole == "Supervisor")
                {
                    project.CurrentMilestone = milestone;
                    project.ProgressPercentage = progress;
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Project progress updated successfully! 🚀";
                }
            }
            return RedirectToAction(nameof(MatchDetails), new { id = projectId });
        }

      
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null) return NotFound();
            return View(project);
        }

   
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
