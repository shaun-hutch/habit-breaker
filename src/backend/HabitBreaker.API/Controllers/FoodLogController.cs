using System.Security.Claims;
using HabitBreaker.API.Data.Repositories;
using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitBreaker.API.Controllers;

[ApiController]
[Route("api/food-logs")]
[Authorize]
public class FoodLogController(IFoodLogRepository repo, IUserHabitRepository habitRepo) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetByDate([FromQuery] int userHabitId, [FromQuery] string date, CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var logDate))
            return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

        if (!await OwnedByUserAsync(userHabitId, ct))
            return Forbid();

        var log = await repo.GetByDateAsync(userHabitId, logDate, ct);
        if (log is null) return NotFound();
        return Ok(Map(log));
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertFoodLogRequest request, CancellationToken ct)
    {
        if (!DateOnly.TryParse(request.LogDate, out var logDate))
            return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

        if (!await OwnedByUserAsync(request.UserHabitId, ct))
            return Forbid();

        var foodLog = new FoodLog
        {
            UserHabitId = request.UserHabitId,
            LogDate = logDate,
            MealEntries = request.MealEntries.Select(me => new MealEntry
            {
                MealType = (MealType)me.MealType,
                Description = me.Description,
                Rating = (MealRating)me.Rating
            }).ToList()
        };

        var result = await repo.UpsertAsync(foodLog, ct);
        return Ok(Map(result));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var log = await repo.GetByIdAsync(id, ct);
        if (log is null) return NotFound();
        if (!await OwnedByUserAsync(log.UserHabitId, ct))
            return Forbid();

        await repo.DeleteAsync(log, ct);
        return NoContent();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int userHabitId,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!await OwnedByUserAsync(userHabitId, ct))
            return Forbid();

        pageSize = Math.Clamp(pageSize, 1, 100);
        var fromDate = from is not null && DateOnly.TryParse(from, out var f) ? f : DateOnly.MinValue;
        var toDate = to is not null && DateOnly.TryParse(to, out var t) ? t : DateOnly.MaxValue;

        var items = await repo.GetHistoryAsync(userHabitId, fromDate, toDate, page, pageSize, ct);
        var total = await repo.GetHistoryCountAsync(userHabitId, fromDate, toDate, ct);
        return Ok(new FoodLogHistoryResponse(items.Select(Map).ToList(), total, page, pageSize));
    }

    private async Task<bool> OwnedByUserAsync(int userHabitId, CancellationToken ct)
    {
        var userHabits = await habitRepo.GetUserHabitsAsync(UserId, ct);
        return userHabits.Any(uh => uh.Id == userHabitId);
    }

    private static FoodLogDto Map(FoodLog fl) => new(
        fl.Id,
        fl.UserHabitId,
        fl.LogDate.ToString("yyyy-MM-dd"),
        fl.CreatedAt,
        fl.UpdatedAt,
        fl.MealEntries.Select(me => new MealEntryDto(
            me.Id,
            (int)me.MealType,
            me.MealType.ToString(),
            me.Description,
            (int)me.Rating,
            me.Rating.ToString())).ToList());
}
