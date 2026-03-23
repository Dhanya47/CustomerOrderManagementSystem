using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly JwtTokenGenerator _jwt;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthDbContext db, JwtTokenGenerator jwt,
        IConfiguration config, ILogger<AuthController> logger)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] Models.RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return Conflict("Username already exists.");

        var adminCode = _config["AdminCode"];
        var role = (!string.IsNullOrEmpty(request.AdminCode) && request.AdminCode == adminCode)
            ? UserRoles.Admin
            : UserRoles.Customer;

        var user = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("New user registered: {Username} with Role: {Role}",
            user.Username, user.Role);

        return CreatedAtAction(nameof(GetProfile), new { }, new { user.UserId, user.Role });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] Models.LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        var hash = user?.PasswordHash ?? BCrypt.Net.BCrypt.HashPassword("dummy");
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, hash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return Unauthorized("Invalid credentials.");
        }

        _logger.LogInformation("User logged in: {Username}", user.Username);
        return Ok(new { token = _jwt.GenerateToken(user!) });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT is stateless — client must discard the token
        // For full logout support, implement a token blacklist
        var username = User.Identity?.Name;
        _logger.LogInformation("User logged out: {Username}", username);
        return Ok("Logged out successfully. Please discard your token.");
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Password changed for user: {Username}", user.Username);
        return Ok("Password changed successfully.");
    }

    [HttpGet("users")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new UserProfileResponse(u.UserId, u.Username, u.Role, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User deleted: {UserId}", id);
        return NoContent();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserProfileResponse(user.UserId, user.Username, user.Role, user.CreatedAt));
    }
}