namespace PMSystem.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClientType { get; set; } = "New"; // New, Old
        public DateTime OnboardedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // NEW FIELDS FOR FILTERS
        public string Industry { get; set; } = string.Empty; // IT, Non-IT, Legal, Payroll, Training
        public string Status { get; set; } = "Active"; // Active, Inactive
        public string Location { get; set; } = string.Empty; // City/State
        public int? AssignedRecruiterId { get; set; }
        public string AssignedRecruiterName { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}