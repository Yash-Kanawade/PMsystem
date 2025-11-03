namespace PMSystem.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        
        public string Status { get; set; } = "Ongoing"; // Ongoing, Completed, OnHold
        public int ProgressPercentage { get; set; } = 0;
        
        public int TeamLeadId { get; set; }
        public string TeamLeadName { get; set; } = string.Empty;
        
        public string TechStack { get; set; } = string.Empty; // JSON string or comma-separated
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<ProjectModule> Modules { get; set; } = new List<ProjectModule>();
    }
}