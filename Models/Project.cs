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

        // ==========================================
        // ALUTH FEATURES: Project Progress Tracker
        // ==========================================
        public int ProgressPercentage { get; set; } = 10; // Default 10% (Proposal Submitted)
        public string? CurrentMilestone { get; set; } = "Proposal Submitted";

        // Relationships
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }
    }
}