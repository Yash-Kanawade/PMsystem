namespace PMSystem.Models
{
    public class ProjectModule
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public string ModuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AssignedToId { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        
        public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public int ProgressPercentage { get; set; } = 0;
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}