namespace PMSystem.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Developer, Designer, QA, etc.
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}