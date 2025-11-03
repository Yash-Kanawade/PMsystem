namespace PMSystem.DTOs
{
    public class CreateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClientType { get; set; } = "New";
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
        public int TotalProjects { get; set; }
        public int OngoingProjects { get; set; }
        public int CompletedProjects { get; set; }
    }
}