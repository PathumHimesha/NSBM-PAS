using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PAS_Project.Controllers {
    public class AccountController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Email service eka link karanawa

        public AccountController(ApplicationDbContext context, IEmailService emailService) {
            _context = context;
            _emailService = emailService;
        }

        // ==========================================
        // PASSWORD HASHING METHOD (Security)
        // ==========================================
        private string HashPassword(string password) {
            using (var sha256 = SHA256.Create()) {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        [HttpGet] public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
                TempData["Error"] = "Email and Password are required!";
                return View();
            }
            
            var claims = new List<Claim>();
            email = email.ToLower().Trim(); 
            string hashedPassword = HashPassword(password);

            // 1. Admin/Module Leader Check
            var admin = await _context.ModuleLeaders.FirstOrDefaultAsync(a => a.Email == email);
            if (admin != null && (admin.Password == hashedPassword || admin.Password == null)) {
                claims.AddRange(new[] { new Claim(ClaimTypes.Name, admin.Name), new Claim("UserId", admin.ModuleLeaderId.ToString()), new Claim(ClaimTypes.Role, "ModuleLeader") });
                if(admin.Password == null) { admin.Password = hashedPassword; await _context.SaveChangesAsync(); } // Save first time password
            }
            // 2. STUDENT CHECK
            else if (email.EndsWith("@students.nsbm.ac.lk")) {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
                if (student != null && (student.Password == hashedPassword || student.Password == null)) {
                    claims.AddRange(new[] { new Claim(ClaimTypes.Name, student.Name), new Claim("UserId", student.StudentId.ToString()), new Claim(ClaimTypes.Role, "Student") });
                    if(student.Password == null) { student.Password = hashedPassword; await _context.SaveChangesAsync(); }
                } 
            } 
            // 3. SUPERVISOR CHECK
            else if (email.EndsWith("@nsbm.ac.lk")) {
                var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);
                if (supervisor != null && (supervisor.Password == hashedPassword || supervisor.Password == null)) {
                    claims.AddRange(new[] { new Claim(ClaimTypes.Name, supervisor.Name), new Claim("UserId", supervisor.SupervisorId.ToString()), new Claim(ClaimTypes.Role, "Supervisor") });
                    if(supervisor.Password == null) { supervisor.Password = hashedPassword; await _context.SaveChangesAsync(); }
                } 
            }

            if (claims.Any()) {
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Invalid Email or Password!";
            return View();
        }

        // ==========================================
        // FORGOT PASSWORD LOGIC
        // ==========================================
        [HttpGet] public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email) {
            if(string.IsNullOrEmpty(email)) return View();
            email = email.ToLower().Trim();

            // Generate a random temporary password
            string tempPassword = "NSBM@" + new Random().Next(1000, 9999);
            string hashedTempPassword = HashPassword(tempPassword);
            bool accountFound = false;
            string userName = "";

            var admin = await _context.ModuleLeaders.FirstOrDefaultAsync(u => u.Email == email);
            if(admin != null) { admin.Password = hashedTempPassword; accountFound = true; userName = admin.Name; }

            var student = await _context.Students.FirstOrDefaultAsync(u => u.Email == email);
            if(student != null) { student.Password = hashedTempPassword; accountFound = true; userName = student.Name; }

            var sup = await _context.Supervisors.FirstOrDefaultAsync(u => u.Email == email);
            if(sup != null) { sup.Password = hashedTempPassword; accountFound = true; userName = sup.Name; }

            if(accountFound) {
                await _context.SaveChangesAsync(); // Update new temp password in DB
                
                string subject = "🔐 Password Reset Request - NSBM PAS";
                string body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px;'>
                        <h2 style='color: #2563eb;'>Hello {userName},</h2>
                        <p>Your password has been successfully reset. Your new temporary password is:</p>
                        <h3 style='background: #f1f5f9; padding: 10px; display: inline-block; border-radius: 5px; color: #0f172a;'>{tempPassword}</h3>
                        <p>Please log in using this password.</p>
                    </div>";

                await _emailService.SendEmailAsync(email, subject, body);
                TempData["Success"] = "A temporary password has been sent to your email! Please check your inbox.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Email address not found in the system.";
            return View();
        }

        public async Task<IActionResult> Logout() { 
            await HttpContext.SignOutAsync("CookieAuth"); 
            return RedirectToAction("Index", "Home"); 
        }
    }
}