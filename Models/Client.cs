namespace PMSystem.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClientType { get; set; } = "New"; // New, Old, Active
        public DateTime OnboardedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}