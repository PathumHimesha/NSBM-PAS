using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models
{
    public class ModuleLeader
    {
        public int ModuleLeaderId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required, EmailAddress]
        public string Email { get; set; }
        public string? Password { get; set; }
    }
}