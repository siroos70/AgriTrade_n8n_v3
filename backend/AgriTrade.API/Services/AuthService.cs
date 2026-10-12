using AgriTrade.Backend.Data;
using Microsoft.AspNetCore.Identity;
using System;

namespace AgriTrade.Backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> Register(RegisterRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly AgriTradeContext _context;

        public AuthService(UserManager<User> userManager, ITokenService tokenService, AgriTradeContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new Exception("Invalid username or password");

            if (!user.Active)
                throw new Exception("User is not active");

            var token = _tokenService.GenerateToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse
            {
                Token = token,
                Username = user.UserName!,
                Name = user.Name,
                Role = user.Role,
                UserId = user.Id
            };
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            var existing = await _userManager.FindByNameAsync(request.Username);
            if (existing != null)
                throw new Exception("Username already exists");

            var user = new User
            {
                UserName = request.Username,
                Email = request.Username,
                Name = request.Name,
                Role = request.Role,
                Active = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, request.Role);

            var token = _tokenService.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                Username = user.UserName!,
                Name = user.Name,
                Role = user.Role,
                UserId = user.Id
            };
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "Seller";
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}