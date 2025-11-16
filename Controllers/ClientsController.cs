using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMSystem.Data;
using PMSystem.DTOs;
using PMSystem.Models;

namespace PMSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetClients(
            [FromQuery] string? industry = null,
            [FromQuery] string? status = null,
            [FromQuery] string? location = null,
            [FromQuery] int? recruiterId = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] string? search = null)
        {
            var query = _context.Clients.Include(c => c.Projects).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(industry))
                query = query.Where(c => c.Industry.ToLower() == industry.ToLower());

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrEmpty(location))
                query = query.Where(c => c.Location.Contains(location));

            if (recruiterId.HasValue)
                query = query.Where(c => c.AssignedRecruiterId == recruiterId.Value);

            if (dateFrom.HasValue)
                query = query.Where(c => c.DateAdded >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(c => c.DateAdded <= dateTo.Value);

            // Apply search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.Name.Contains(search) || 
                    c.CompanyName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Location.Contains(search));
            }

            var clients = await query.Select(c => new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                CompanyName = c.CompanyName,
                Email = c.Email,
                Phone = c.Phone,
                ClientType = c.ClientType,
                OnboardedDate = c.OnboardedDate,
                IsActive = c.IsActive,
                Industry = c.Industry,
                Status = c.Status,
                Location = c.Location,
                AssignedRecruiterId = c.AssignedRecruiterId,
                AssignedRecruiterName = c.AssignedRecruiterName,
                DateAdded = c.DateAdded,
                TotalProjects = c.Projects.Count,
                OngoingProjects = c.Projects.Count(p => p.Status == "Ongoing"),
                CompletedProjects = c.Projects.Count(p => p.Status == "Completed")
            }).ToListAsync();

            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound(new { message = "Client not found" });

            var response = new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Phone = client.Phone,
                ClientType = client.ClientType,
                OnboardedDate = client.OnboardedDate,
                IsActive = client.IsActive,
                Industry = client.Industry,
                Status = client.Status,
                Location = client.Location,
                AssignedRecruiterId = client.AssignedRecruiterId,
                AssignedRecruiterName = client.AssignedRecruiterName,
                DateAdded = client.DateAdded,
                TotalProjects = client.Projects.Count,
                OngoingProjects = client.Projects.Count(p => p.Status == "Ongoing"),
                CompletedProjects = client.Projects.Count(p => p.Status == "Completed")
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            // Validate assigned recruiter exists if provided
            if (request.AssignedRecruiterId.HasValue)
            {
                var recruiterExists = await _context.Users
                    .AnyAsync(u => u.Id == request.AssignedRecruiterId.Value && u.IsActive);
                
                if (!recruiterExists)
                {
                    return BadRequest(new { message = $"Recruiter with ID {request.AssignedRecruiterId.Value} does not exist or is inactive" });
                }

                // Get recruiter name if not provided
                if (string.IsNullOrEmpty(request.AssignedRecruiterName))
                {
                    var recruiter = await _context.Users.FindAsync(request.AssignedRecruiterId.Value);
                    request.AssignedRecruiterName = recruiter?.Username ?? string.Empty;
                }
            }

            // Check if email already exists
            var emailExists = await _context.Clients.AnyAsync(c => c.Email == request.Email);
            if (emailExists)
            {
                return BadRequest(new { message = "A client with this email already exists" });
            }

            var client = new Client
            {
                Name = request.Name,
                CompanyName = request.CompanyName,
                Email = request.Email,
                Phone = request.Phone,
                ClientType = request.ClientType,
                Industry = request.Industry,
                Status = request.Status,
                Location = request.Location,
                AssignedRecruiterId = request.AssignedRecruiterId,
                AssignedRecruiterName = request.AssignedRecruiterName
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] CreateClientRequest request)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound(new { message = "Client not found" });

            // Validate assigned recruiter exists if provided
            if (request.AssignedRecruiterId.HasValue)
            {
                var recruiterExists = await _context.Users
                    .AnyAsync(u => u.Id == request.AssignedRecruiterId.Value && u.IsActive);
                
                if (!recruiterExists)
                {
                    return BadRequest(new { message = $"Recruiter with ID {request.AssignedRecruiterId.Value} does not exist or is inactive" });
                }

                // Get recruiter name if not provided
                if (string.IsNullOrEmpty(request.AssignedRecruiterName))
                {
                    var recruiter = await _context.Users.FindAsync(request.AssignedRecruiterId.Value);
                    request.AssignedRecruiterName = recruiter?.Username ?? string.Empty;
                }
            }

            // Check if email already exists for another client
            var emailExists = await _context.Clients
                .AnyAsync(c => c.Email == request.Email && c.Id != id);
            if (emailExists)
            {
                return BadRequest(new { message = "A client with this email already exists" });
            }

            client.Name = request.Name;
            client.CompanyName = request.CompanyName;
            client.Email = request.Email;
            client.Phone = request.Phone;
            client.ClientType = request.ClientType;
            client.Industry = request.Industry;
            client.Status = request.Status;
            client.Location = request.Location;
            client.AssignedRecruiterId = request.AssignedRecruiterId;
            client.AssignedRecruiterName = request.AssignedRecruiterName;

            await _context.SaveChangesAsync();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (client == null)
                return NotFound(new { message = "Client not found" });

            // Check if client has any projects
            if (client.Projects.Any())
            {
                return BadRequest(new { 
                    message = "Cannot delete client with existing projects. Please delete or reassign all projects first.",
                    projectCount = client.Projects.Count
                });
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Client deleted successfully" });
        }
    }
}