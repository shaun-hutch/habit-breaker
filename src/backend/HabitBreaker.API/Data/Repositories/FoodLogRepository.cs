using HabitBreaker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitBreaker.API.Data.Repositories;

public class FoodLogRepository(ApplicationDbContext db) : IFoodLogRepository
{
    public async Task<FoodLog?> GetByDateAsync(int userHabitId, DateOnly date, CancellationToken ct = default) =>
        await db.FoodLogs
            .Include(fl => fl.MealEntries)
            .FirstOrDefaultAsync(fl => fl.UserHabitId == userHabitId && fl.LogDate == date, ct);

    public async Task<FoodLog?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.FoodLogs
            .Include(fl => fl.MealEntries)
            .FirstOrDefaultAsync(fl => fl.Id == id, ct);

    public async Task<List<FoodLog>> GetHistoryAsync(int userHabitId, DateOnly from, DateOnly to, int page, int pageSize, CancellationToken ct = default) =>
        await db.FoodLogs
            .Include(fl => fl.MealEntries)
            .Where(fl => fl.UserHabitId == userHabitId && fl.LogDate >= from && fl.LogDate <= to)
            .OrderByDescending(fl => fl.LogDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetHistoryCountAsync(int userHabitId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await db.FoodLogs
            .CountAsync(fl => fl.UserHabitId == userHabitId && fl.LogDate >= from && fl.LogDate <= to, ct);

    public async Task<FoodLog> UpsertAsync(FoodLog foodLog, CancellationToken ct = default)
    {
        var existing = await GetByDateAsync(foodLog.UserHabitId, foodLog.LogDate, ct);
        if (existing is null)
        {
            db.FoodLogs.Add(foodLog);
        }
        else
        {
            // Local wins: remove all existing meal entries and replace
            db.MealEntries.RemoveRange(existing.MealEntries);
            existing.MealEntries = foodLog.MealEntries;
            existing.UpdatedAt = DateTime.UtcNow;
            foodLog = existing;
        }
        await db.SaveChangesAsync(ct);
        return foodLog;
    }

    public async Task DeleteAsync(FoodLog foodLog, CancellationToken ct = default)
    {
        db.FoodLogs.Remove(foodLog);
        await db.SaveChangesAsync(ct);
    }
}
