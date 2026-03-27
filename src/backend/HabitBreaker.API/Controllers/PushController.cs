using System.Security.Claims;
using HabitBreaker.API.Data;
using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using HabitBreaker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HabitBreaker.API.Controllers;

[ApiController]
[Route("api/push")]
[Authorize]
public class PushController(
    IPushNotificationService pushService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscribeRequest request, CancellationToken ct)
    {
        await pushService.SaveSubscriptionAsync(UserId, request.Endpoint, request.P256dh, request.Auth, ct);
        return NoContent();
    }

    [HttpDelete("subscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] PushUnsubscribeRequest request, CancellationToken ct)
    {
        await pushService.RemoveSubscriptionAsync(UserId, request.Endpoint, ct);
        return NoContent();
    }

    [HttpPut("reminder-time")]
    public async Task<IActionResult> UpdateReminderTime([FromBody] UpdateReminderTimeRequest request)
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is null) return NotFound();

        if (request.ReminderTime is null)
        {
            user.PreferredReminderTime = null;
        }
        else
        {
            if (!TimeOnly.TryParseExact(request.ReminderTime, "HH:mm", out var time))
                return BadRequest(new { error = "Invalid time format. Use HH:mm." });
            user.PreferredReminderTime = time;
        }

        await userManager.UpdateAsync(user);
        return NoContent();
    }
}
