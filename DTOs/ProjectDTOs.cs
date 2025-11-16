namespace PMSystem.DTOs
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int TeamLeadId { get; set; }
        public string TeamLeadName { get; set; } = string.Empty;
        public string TechStack { get; set; } = string.Empty;
    }

    public class UpdateProjectRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string? Status { get; set; }
        public int? ProgressPercentage { get; set; }
        public string? TechStack { get; set; }
    }

    public class UpdateProgressRequest
    {
        public int? ProgressPercentage { get; set; }
    }

    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public int TeamLeadId { get; set; }
        public string TeamLeadName { get; set; } = string.Empty;
        public string TechStack { get; set; } = string.Empty;
        public List<TeamMemberResponse> TeamMembers { get; set; } = new();
        public List<ModuleResponse> Modules { get; set; } = new();
    }

    public class TeamMemberResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
    }

    public class ModuleResponse
    {
        public int Id { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
    }

   public class AddTeamMemberRequest
{
    public int UserId { get; set; }  // Required: User must exist
    public string Role { get; set; } = string.Empty;  // Role in this project
}
    public class AddModuleRequest
    {
        public string ModuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AssignedToId { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
    }

    
}