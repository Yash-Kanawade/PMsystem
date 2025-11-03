using PMSystem.DTOs;
using PMSystem.Models;

namespace PMSystem.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> Login(LoginRequest request);
        Task<User?> Register(RegisterRequest request);
        string GenerateJwtToken(User user);
    }
}