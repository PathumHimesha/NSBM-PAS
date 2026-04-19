using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Abstract { get; set; }

        [Required]
        public string TechnicalStack { get; set; }

        [Required]
        public string ResearchArea { get; set; }

        public ProjectStatus Status { get; set; }

        public string? ProposalFilePath { get; set; }


        public int ProgressPercentage { get; set; } = 10; 
        public string? CurrentMilestone { get; set; } = "Proposal Submitted";

       
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }
    }
}
