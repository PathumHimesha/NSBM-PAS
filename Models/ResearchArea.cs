using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models
{
    public class ResearchArea
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}