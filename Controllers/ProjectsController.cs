using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMSystem.Data;
using PMSystem.DTOs;
using PMSystem.Models;

namespace PMSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects(
            [FromQuery] int? clientId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null)
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.TeamMembers)
                .Include(p => p.Modules)
                .AsQueryable();

            if (clientId.HasValue)
                query = query.Where(p => p.ClientId == clientId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => 
                    p.Name.Contains(search) || 
                    p.Description.Contains(search) ||
                    p.Client.CompanyName.Contains(search));

            var projects = await query.Select(p => new ProjectResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ClientId = p.ClientId,
                ClientName = p.Client.CompanyName,
                StartDate = p.StartDate,
                ExpectedEndDate = p.ExpectedEndDate,
                ActualEndDate = p.ActualEndDate,
                Status = p.Status,
                ProgressPercentage = p.ProgressPercentage,
                TeamLeadId = p.TeamLeadId,
                TeamLeadName = p.TeamLeadName,
                TechStack = p.TechStack,
                TeamMembers = p.TeamMembers.Select(tm => new TeamMemberResponse
                {
                    Id = tm.Id,
                    Name = tm.Name,
                    Email = tm.Email,
                    Role = tm.Role,
                    JoinedDate = tm.JoinedDate
                }).ToList(),
                Modules = p.Modules.Select(m => new ModuleResponse
                {
                    Id = m.Id,
                    ModuleName = m.ModuleName,
                    Description = m.Description,
                    AssignedToName = m.AssignedToName,
                    Status = m.Status,
                    ProgressPercentage = m.ProgressPercentage
                }).ToList()
            }).ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.TeamMembers)
                .Include(p => p.Modules)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ClientId = project.ClientId,
                ClientName = project.Client.CompanyName,
                StartDate = project.StartDate,
                ExpectedEndDate = project.ExpectedEndDate,
                ActualEndDate = project.ActualEndDate,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                TeamLeadId = project.TeamLeadId,
                TeamLeadName = project.TeamLeadName,
                TechStack = project.TechStack,
                TeamMembers = project.TeamMembers.Select(tm => new TeamMemberResponse
                {
                    Id = tm.Id,
                    Name = tm.Name,
                    Email = tm.Email,
                    Role = tm.Role,
                    JoinedDate = tm.JoinedDate
                }).ToList(),
                Modules = project.Modules.Select(m => new ModuleResponse
                {
                    Id = m.Id,
                    ModuleName = m.ModuleName,
                    Description = m.Description,
                    AssignedToName = m.AssignedToName,
                    Status = m.Status,
                    ProgressPercentage = m.ProgressPercentage
                }).ToList()
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            // Validate client exists
            var clientExists = await _context.Clients
                .AnyAsync(c => c.Id == request.ClientId && c.IsActive);
            
            if (!clientExists)
            {
                return BadRequest(new { message = $"Client with ID {request.ClientId} does not exist or is inactive" });
            }

            // Validate team lead exists
            var teamLeadExists = await _context.Users
                .AnyAsync(u => u.Id == request.TeamLeadId && u.IsActive);
            
            if (!teamLeadExists)
            {
                return BadRequest(new { message = $"Team lead with ID {request.TeamLeadId} does not exist or is inactive" });
            }

            // Get team lead name if not provided
            if (string.IsNullOrEmpty(request.TeamLeadName))
            {
                var teamLead = await _context.Users.FindAsync(request.TeamLeadId);
                request.TeamLeadName = teamLead?.Username ?? string.Empty;
            }

            // Validate dates
            if (request.ExpectedEndDate.HasValue && request.ExpectedEndDate.Value < request.StartDate)
            {
                return BadRequest(new { message = "Expected end date cannot be before start date" });
            }

            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                ClientId = request.ClientId,
                StartDate = request.StartDate,
                ExpectedEndDate = request.ExpectedEndDate,
                TeamLeadId = request.TeamLeadId,
                TeamLeadName = request.TeamLeadName,
                TechStack = request.TechStack,
                Status = "Ongoing"
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Return DTO instead of entity
            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ClientId = project.ClientId,
                ClientName = (await _context.Clients.FindAsync(project.ClientId))?.CompanyName ?? "",
                StartDate = project.StartDate,
                ExpectedEndDate = project.ExpectedEndDate,
                ActualEndDate = project.ActualEndDate,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                TeamLeadId = project.TeamLeadId,
                TeamLeadName = project.TeamLeadName,
                TechStack = project.TechStack,
                TeamMembers = new List<TeamMemberResponse>(),
                Modules = new List<ModuleResponse>()
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, response);
        }

        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressRequest request)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null)
                return NotFound(new { message = "Project not found" });

            // Validate progress percentage
            if (request.ProgressPercentage.HasValue)
            {
                if (request.ProgressPercentage.Value < 0 || request.ProgressPercentage.Value > 100)
                {
                    return BadRequest(new { message = "Progress percentage must be between 0 and 100" });
                }
                
                project.ProgressPercentage = request.ProgressPercentage.Value;
            }

            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            // Return DTO
            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ClientId = project.ClientId,
                ClientName = project.Client.CompanyName,
                StartDate = project.StartDate,
                ExpectedEndDate = project.ExpectedEndDate,
                ActualEndDate = project.ActualEndDate,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                TeamLeadId = project.TeamLeadId,
                TeamLeadName = project.TeamLeadName,
                TechStack = project.TechStack,
                TeamMembers = new List<TeamMemberResponse>(),
                Modules = new List<ModuleResponse>()
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null)
                return NotFound(new { message = "Project not found" });

            // Validate progress percentage
            if (request.ProgressPercentage.HasValue)
            {
                if (request.ProgressPercentage.Value < 0 || request.ProgressPercentage.Value > 100)
                {
                    return BadRequest(new { message = "Progress percentage must be between 0 and 100" });
                }
            }

            // Validate dates
            if (request.ExpectedEndDate.HasValue && request.ExpectedEndDate.Value < project.StartDate)
            {
                return BadRequest(new { message = "Expected end date cannot be before start date" });
            }

            if (request.ActualEndDate.HasValue && request.ActualEndDate.Value < project.StartDate)
            {
                return BadRequest(new { message = "Actual end date cannot be before start date" });
            }

            if (!string.IsNullOrEmpty(request.Name))
                project.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                project.Description = request.Description;
            if (request.ExpectedEndDate.HasValue)
                project.ExpectedEndDate = request.ExpectedEndDate;
            if (request.ActualEndDate.HasValue)
                project.ActualEndDate = request.ActualEndDate;
            if (!string.IsNullOrEmpty(request.Status))
                project.Status = request.Status;
            if (request.ProgressPercentage.HasValue)
                project.ProgressPercentage = request.ProgressPercentage.Value;
            if (!string.IsNullOrEmpty(request.TechStack))
                project.TechStack = request.TechStack;

            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            // Return DTO
            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ClientId = project.ClientId,
                ClientName = project.Client.CompanyName,
                StartDate = project.StartDate,
                ExpectedEndDate = project.ExpectedEndDate,
                ActualEndDate = project.ActualEndDate,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                TeamLeadId = project.TeamLeadId,
                TeamLeadName = project.TeamLeadName,
                TechStack = project.TechStack,
                TeamMembers = new List<TeamMemberResponse>(),
                Modules = new List<ModuleResponse>()
            };

            return Ok(response);
        }

        [HttpPost("{id}/team-members")]
public async Task<IActionResult> AddTeamMember(int id, [FromBody] AddTeamMemberRequest request)
{
    var project = await _context.Projects.FindAsync(id);
    if (project == null)
        return NotFound(new { message = "Project not found" });

    // Validate user exists
    var user = await _context.Users
        .Where(u => u.Id == request.UserId && u.IsActive)
        .FirstOrDefaultAsync();
    
    if (user == null)
    {
        return BadRequest(new { message = $"User with ID {request.UserId} does not exist or is inactive" });
    }

    // Check if user is already a team member in this project
    var alreadyExists = await _context.TeamMembers
        .AnyAsync(tm => tm.ProjectId == id && tm.UserId == request.UserId);
    
    if (alreadyExists)
    {
        return BadRequest(new { message = $"{user.Username} is already a team member of this project" });
    }

    var teamMember = new TeamMember
    {
        ProjectId = id,
        UserId = user.Id,
        Name = user.Username,
        Email = user.Email,
        Role = request.Role
    };

    _context.TeamMembers.Add(teamMember);
    await _context.SaveChangesAsync();

    // Return DTO
    var response = new TeamMemberResponse
    {
        Id = teamMember.Id,
        Name = teamMember.Name,
        Email = teamMember.Email,
        Role = teamMember.Role,
        JoinedDate = teamMember.JoinedDate
    };

    return Ok(response);
}

        [HttpPost("{id}/modules")]
        public async Task<IActionResult> AddModule(int id, [FromBody] AddModuleRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            // Validate assigned team member exists in this project
            if (request.AssignedToId > 0)
            {
                var teamMemberExists = await _context.TeamMembers
                    .AnyAsync(tm => tm.Id == request.AssignedToId && tm.ProjectId == id && tm.IsActive);
                
                if (!teamMemberExists)
                {
                    return BadRequest(new { message = $"Team member with ID {request.AssignedToId} does not exist in this project or is inactive" });
                }

                // Get team member name if not provided
                if (string.IsNullOrEmpty(request.AssignedToName))
                {
                    var teamMember = await _context.TeamMembers.FindAsync(request.AssignedToId);
                    request.AssignedToName = teamMember?.Name ?? string.Empty;
                }
            }

            // Check if module with same name already exists in this project
            var moduleExists = await _context.ProjectModules
                .AnyAsync(m => m.ProjectId == id && m.ModuleName == request.ModuleName);
            
            if (moduleExists)
            {
                return BadRequest(new { message = "A module with this name already exists in this project" });
            }

            var module = new ProjectModule
            {
                ProjectId = id,
                ModuleName = request.ModuleName,
                Description = request.Description,
                AssignedToId = request.AssignedToId,
                AssignedToName = request.AssignedToName,
                Status = "NotStarted"
            };

            _context.ProjectModules.Add(module);
            await _context.SaveChangesAsync();

            // Return DTO instead of entity
            var response = new ModuleResponse
            {
                Id = module.Id,
                ModuleName = module.ModuleName,
                Description = module.Description,
                AssignedToName = module.AssignedToName,
                Status = module.Status,
                ProgressPercentage = module.ProgressPercentage
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.TeamMembers)
                .Include(p => p.Modules)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null)
                return NotFound(new { message = "Project not found" });

            var teamMemberCount = project.TeamMembers.Count;
            var moduleCount = project.Modules.Count;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Project deleted successfully",
                deletedTeamMembers = teamMemberCount,
                deletedModules = moduleCount
            });
        }

        // Helper method to validate email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}