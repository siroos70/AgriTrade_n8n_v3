using AgriTrade.Backend.Data;
using AgriTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly AgriTradeContext _context;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration config,
            AgriTradeContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "نام کاربری یا رمز عبور نادرست" });

            if (!user.Active)
                return Unauthorized(new { message = "کاربر غیرفعال است" });

            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                username = user.UserName,
                name = user.Name,
                role = user.Role,
                userId = user.Id
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existing = await _userManager.FindByNameAsync(request.Username);
            if (existing != null)
                return BadRequest(new { message = "نام کاربری قبلاً وجود دارد" });

            var user = new User
            {
                UserName = request.Username,
                Email = request.Username,
                Name = request.Name,
                Role = request.Role,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await _userManager.AddToRoleAsync(user, request.Role);

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                username = user.UserName,
                name = user.Name,
                role = user.Role,
                userId = user.Id
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                username = user.UserName,
                name = user.Name,
                role = user.Role,
                userId = user.Id,
                roles = roles
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = _config["Jwt:Key"] ?? "agritrade-super-secret-key-256-bits-min-length-CHANGE-IN-PROD";
            var issuer = _config["Jwt:Issuer"] ?? "AgriTrade";
            var audience = _config["Jwt:Audience"] ?? "AgriTrade";
            var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]?.ToString() ?? "120");

            var securityKey = new System.Security.Cryptography.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
            var credentials = new System.IdentityModel.Tokens.Jwt.SigningCredentials(securityKey, System.IdentityModel.Tokens.Jwt.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, user.Name),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, System.Guid.NewGuid().ToString())
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
