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
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

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

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

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
            return Ok(project);
        }

        [HttpPost("{id}/team-members")]
        public async Task<IActionResult> AddTeamMember(int id, [FromBody] AddTeamMemberRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            var teamMember = new TeamMember
            {
                ProjectId = id,
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            return Ok(teamMember);
        }

        [HttpPost("{id}/modules")]
        public async Task<IActionResult> AddModule(int id, [FromBody] AddModuleRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

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

            return Ok(module);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project deleted successfully" });
        }
    }
}