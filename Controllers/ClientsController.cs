using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMSystem.Data;
using PMSystem.DTOs;
using PMSystem.Models;

namespace PMSystem.Controllers
{
    [Authorize]
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
            [FromQuery] string? filter = null,
            [FromQuery] string? search = null)
        {
            var query = _context.Clients.Include(c => c.Projects).AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.Name.Contains(search) || 
                    c.CompanyName.Contains(search) ||
                    c.Email.Contains(search));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(filter))
            {
                query = filter.ToLower() switch
                {
                    "new" => query.Where(c => c.ClientType == "New"),
                    "old" => query.Where(c => c.ClientType == "Old"),
                    "active" => query.Where(c => c.IsActive),
                    "mostly" => query.OrderByDescending(c => c.Projects.Count).AsQueryable(),
                    _ => query
                };
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
                TotalProjects = client.Projects.Count,
                OngoingProjects = client.Projects.Count(p => p.Status == "Ongoing"),
                CompletedProjects = client.Projects.Count(p => p.Status == "Completed")
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            var client = new Client
            {
                Name = request.Name,
                CompanyName = request.CompanyName,
                Email = request.Email,
                Phone = request.Phone,
                ClientType = request.ClientType
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

            client.Name = request.Name;
            client.CompanyName = request.CompanyName;
            client.Email = request.Email;
            client.Phone = request.Phone;
            client.ClientType = request.ClientType;

            await _context.SaveChangesAsync();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound(new { message = "Client not found" });

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Client deleted successfully" });
        }
    }
}