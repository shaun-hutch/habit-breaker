using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using HabitBreaker.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HabitBreaker.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return await IssueTokens(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized(new { error = "Invalid credentials." });

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut) return StatusCode(429, new { error = "Account locked. Try again later." });
            return Unauthorized(new { error = "Invalid credentials." });
        }

        return await IssueTokens(user);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var refreshToken = await tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (refreshToken is null) return Unauthorized(new { error = "Invalid or expired refresh token." });

        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return await IssueTokens(refreshToken.User);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return NoContent();
    }

    private async Task<IActionResult> IssueTokens(ApplicationUser user)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);
        var dto = new UserDto(user.Id, user.Email!, user.PreferredReminderTime?.ToString("HH:mm"));
        return Ok(new AuthResponse(accessToken, refreshToken.Token, dto));
    }
}
