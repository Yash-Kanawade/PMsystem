namespace PMSystem.DTOs
{
    public class CreateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClientType { get; set; } = "New";
        public string Industry { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string Location { get; set; } = string.Empty;
        public int? AssignedRecruiterId { get; set; }
        public string AssignedRecruiterName { get; set; } = string.Empty;
    }

    public class ClientResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClientType { get; set; } = string.Empty;
        public DateTime OnboardedDate { get; set; }
        public bool IsActive { get; set; }
        public string Industry { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? AssignedRecruiterId { get; set; }
        public string AssignedRecruiterName { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public int TotalProjects { get; set; }
        public int OngoingProjects { get; set; }
        public int CompletedProjects { get; set; }
    }
}