using System.Security.Claims;
using HabitBreaker.API.Data.Repositories;
using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitBreaker.API.Controllers;

[ApiController]
[Route("api/habits")]
[Authorize]
public class HabitsController(IUserHabitRepository repo) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var habits = await repo.GetAllHabitsAsync(ct);
        return Ok(habits.Select(h => new HabitDto(h.Id, h.Name, h.Description, h.DefaultDurationDays)));
    }

    [HttpPost("enroll")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request, CancellationToken ct)
    {
        var existing = await repo.GetActiveUserHabitAsync(UserId, request.HabitId, ct);
        if (existing is not null)
            return Conflict(new { error = "Already enrolled in this habit." });

        var userHabit = new UserHabit
        {
            UserId = UserId,
            HabitId = request.HabitId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DurationDays = request.DurationDays ?? 66
        };
        var created = await repo.EnrollAsync(userHabit, ct);
        return CreatedAtAction(nameof(GetMine), MapUserHabit(created));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userHabits = await repo.GetUserHabitsAsync(UserId, ct);
        return Ok(userHabits.Select(MapUserHabit));
    }

    private static UserHabitDto MapUserHabit(UserHabit uh)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayNumber = Math.Clamp((today.DayNumber - uh.StartDate.DayNumber) + 1, 1, uh.DurationDays);
        var progress = Math.Round((double)dayNumber / uh.DurationDays * 100, 1);
        return new UserHabitDto(
            uh.Id,
            new HabitDto(uh.Habit.Id, uh.Habit.Name, uh.Habit.Description, uh.Habit.DefaultDurationDays),
            uh.StartDate.ToString("yyyy-MM-dd"),
            uh.DurationDays,
            dayNumber,
            uh.DurationDays,
            progress,
            dayNumber >= uh.DurationDays);
    }
}
