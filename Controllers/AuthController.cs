using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMSystem.Data;
using PMSystem.DTOs;
using PMSystem.Models;

namespace PMSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // FOR TESTING ONLY - Create test users
        [HttpPost("create-test-users")]
        public async Task<IActionResult> CreateTestUsers()
        {
            try
            {
                // Check if users already exist
                var existingUsers = await _context.Users.AnyAsync();
                if (existingUsers)
                {
                    return Ok(new { message = "Test users already exist" });
                }

                var users = new List<User>
                {
                    new User
                    {
                        Username = "manager1",
                        Email = "manager1@example.com",
                        Password = "Password123",
                        Role = "Manager",
                        IsActive = true
                    },
                    new User
                    {
                        Username = "employee1",
                        Email = "employee1@example.com",
                        Password = "Password123",
                        Role = "Employee",
                        IsActive = true
                    }
                };

                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Test users created successfully",
                    users = users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.Role
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating test users", error = ex.Message });
            }
        }

        // FOR TESTING - Set session manually
        [HttpPost("set-session")]
        public IActionResult SetSession([FromBody] SetSessionRequest request)
        {
            HttpContext.Session.SetInt32("UserId", request.UserId);
            HttpContext.Session.SetString("Username", request.Username);
            HttpContext.Session.SetString("Role", request.Role);

            return Ok(new
            {
                message = "Session set successfully",
                userId = request.UserId,
                username = request.Username,
                role = request.Role
            });
        }

        // Check current session
        [HttpGet("current-session")]
        public IActionResult GetCurrentSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Ok(new { message = "No active session" });
            }

            return Ok(new
            {
                userId = userId.Value,
                username = username,
                role = role
            });
        }

        // Clear session
        [HttpPost("clear-session")]
        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Session cleared successfully" });
        }

        // Get all users (for testing)
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        // Get all active users (for dropdown lists)
[HttpGet("active-users")]
public async Task<IActionResult> GetActiveUsers([FromQuery] string? role = null)
{
    var query = _context.Users.Where(u => u.IsActive).AsQueryable();

    if (!string.IsNullOrEmpty(role))
    {
        query = query.Where(u => u.Role.ToLower() == role.ToLower());
    }

    var users = await query
        .Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.Role
        })
        .ToListAsync();

    return Ok(users);
}

// Get user by ID
[HttpGet("users/{id}")]
public async Task<IActionResult> GetUserById(int id)
{
    var user = await _context.Users
        .Where(u => u.Id == id)
        .Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.Role,
            u.IsActive,
            u.CreatedAt
        })
        .FirstOrDefaultAsync();

    if (user == null)
        return NotFound(new { message = "User not found" });

    return Ok(user);
}
    }

    public class SetSessionRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    
}
