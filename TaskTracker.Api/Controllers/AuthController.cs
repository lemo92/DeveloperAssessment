using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokens;

        public AuthController(AppDbContext db, TokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                return Conflict("Username already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                Role = "User"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return CreatedAtAction(null, new { user.Id, user.Username, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
                return Unauthorized();

            var access = _tokens.GenerateAccessToken(user.Id, user.Username, user.Role);
            var (refresh, expiresAt) = _tokens.GenerateRefreshToken();

            user.RefreshToken = refresh;
            user.RefreshTokenExpiresAt = expiresAt;
            await _db.SaveChangesAsync();

            return Ok(new { accessToken = access, refreshToken = refresh, refreshExpiresAt = expiresAt });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
                return Unauthorized();

            var access = _tokens.GenerateAccessToken(user.Id, user.Username, user.Role);
            var (refresh, expiresAt) = _tokens.GenerateRefreshToken();
            user.RefreshToken = refresh;
            user.RefreshTokenExpiresAt = expiresAt;
            await _db.SaveChangesAsync();
            return Ok(new { accessToken = access, refreshToken = refresh, refreshExpiresAt = expiresAt });
        }
    }

    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string Username, string Password);
    public record RefreshRequest(string RefreshToken);
}


