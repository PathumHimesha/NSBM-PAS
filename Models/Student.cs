using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models 
{
    public class Student 
    {
        public int StudentId { get; set; }
        
        [Required] 
        public string Name { get; set; }
        
        [Required, EmailAddress] 
        public string Email { get; set; }
        

        public string? ProfileImagePath { get; set; }
        public string? GitHubLink { get; set; }
        public string? LinkedInLink { get; set; }
        
        public ICollection<Project> Projects { get; set; }
        public string? Password { get; set; }
    }
}
