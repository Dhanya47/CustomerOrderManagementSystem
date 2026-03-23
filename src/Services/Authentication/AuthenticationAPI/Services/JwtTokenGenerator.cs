using AuthService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services;

public class JwtTokenGenerator
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryHours;

    public JwtTokenGenerator(IConfiguration config)
    {
        var rawKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        _expiryHours = int.TryParse(config["Jwt:ExpiryHours"], out var h) ? h : 1;

        if (rawKey.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters for HMAC-SHA256.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
    }

    public string GenerateToken(User user)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId),           // stable unique ID
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),  // human-readable
            new Claim(ClaimTypes.Role, user.Role),                          // works with [Authorize(Roles=)]
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}