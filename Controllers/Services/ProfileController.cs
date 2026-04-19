using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PAS_Project.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // PASSWORD HASHING METHOD 
        // ==========================================
        private string HashPassword(string password) {
            using (var sha256 = SHA256.Create()) {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (int.TryParse(userIdClaim, out int id))
            {
                ViewBag.Role = role;
                if (role == "Student")
                {
                    var student = await _context.Students.FindAsync(id);
                    ViewBag.Name = student.Name;
                    ViewBag.Email = student.Email;
                    ViewBag.ProfileImagePath = student.ProfileImagePath;
                    ViewBag.GitHubLink = student.GitHubLink;
                    ViewBag.LinkedInLink = student.LinkedInLink;
                }
                else if (role == "Supervisor")
                {
                    var supervisor = await _context.Supervisors.FindAsync(id);
                    ViewBag.Name = supervisor.Name;
                    ViewBag.Email = supervisor.Email;
                    ViewBag.ProfileImagePath = supervisor.ProfileImagePath;
                    ViewBag.Bio = supervisor.Bio;
                    ViewBag.LinkedInLink = supervisor.LinkedInLink;
                }
                else if (role == "ModuleLeader")
                {
                    var admin = await _context.ModuleLeaders.FindAsync(id);
                    ViewBag.Name = admin.Name;
                    ViewBag.Email = admin.Email;
                }
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string? github, string? linkedin, string? bio, IFormFile? profileImage)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (int.TryParse(userIdClaim, out int id))
            {
                string? imagePath = null;
                if (profileImage != null && profileImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/profiles");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) { 
                        await profileImage.CopyToAsync(fileStream); 
                    }
                    imagePath = "/uploads/profiles/" + uniqueFileName;
                }

                if (role == "Student")
                {
                    var student = await _context.Students.FindAsync(id);
                    if (student != null) {
                        if (github != null) student.GitHubLink = github;
                        if (linkedin != null) student.LinkedInLink = linkedin;
                        if (imagePath != null) student.ProfileImagePath = imagePath;
                        _context.Update(student);
                    }
                }
                else if (role == "Supervisor")
                {
                    var supervisor = await _context.Supervisors.FindAsync(id);
                    if (supervisor != null) {
                        if (bio != null) supervisor.Bio = bio;
                        if (linkedin != null) supervisor.LinkedInLink = linkedin;
                        if (imagePath != null) supervisor.ProfileImagePath = imagePath;
                        _context.Update(supervisor);
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully! 🔥";
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // ALUTH: Change Password Logic
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword) {
                TempData["Error"] = "New password and confirmation do not match!";
                return RedirectToAction("Index");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (int.TryParse(userIdClaim, out int id))
            {
                string hashedCurrent = HashPassword(currentPassword);
                string hashedNew = HashPassword(newPassword);
                bool success = false;

                if (role == "Student") {
                    var student = await _context.Students.FindAsync(id);
                    if (student != null && student.Password == hashedCurrent) {
                        student.Password = hashedNew;
                        success = true;
                    }
                } 
                else if (role == "Supervisor") {
                    var supervisor = await _context.Supervisors.FindAsync(id);
                    if (supervisor != null && supervisor.Password == hashedCurrent) {
                        supervisor.Password = hashedNew;
                        success = true;
                    }
                }
                else if (role == "ModuleLeader") {
                    var admin = await _context.ModuleLeaders.FindAsync(id);
                    if (admin != null && admin.Password == hashedCurrent) {
                        admin.Password = hashedNew;
                        success = true;
                    }
                }

                if (success) {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Password changed successfully! 🔐 Please use your new password next time.";
                } else {
                    TempData["Error"] = "The current password you entered is incorrect!";
                }
            }
            return RedirectToAction("Index");
        }
    }
}