using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HabitBreaker.API.Data;
using HabitBreaker.API.Models;
using HabitBreaker.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HabitBreaker.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, CancellationToken ct = default);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string token, CancellationToken ct = default);
}

public class TokenService(IOptions<JwtSettings> jwtOptions, ApplicationDbContext db) : ITokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public string GenerateAccessToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, CancellationToken ct = default)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        };
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken ct = default) =>
        await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, ct);

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        var rt = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);
        if (rt is not null)
        {
            rt.IsRevoked = true;
            await db.SaveChangesAsync(ct);
        }
    }
}
