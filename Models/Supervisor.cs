using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models 
{
    public class Supervisor 
    {
        public int SupervisorId { get; set; }
        
        [Required] 
        public string Name { get; set; }
        
        [Required, EmailAddress] 
        public string Email { get; set; }
        
        public string? PreferredResearchAreas { get; set; } 
        

        public string? ProfileImagePath { get; set; }
        public string? LinkedInLink { get; set; }
        public string? Bio { get; set; }
        
        public ICollection<Project> Projects { get; set; }
        public string? Password { get; set; }
    }
}
