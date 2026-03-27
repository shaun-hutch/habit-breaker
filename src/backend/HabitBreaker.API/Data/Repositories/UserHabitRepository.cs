using HabitBreaker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitBreaker.API.Data.Repositories;

public class UserHabitRepository(ApplicationDbContext db) : IUserHabitRepository
{
    public async Task<List<Habit>> GetAllHabitsAsync(CancellationToken ct = default) =>
        await db.Habits.ToListAsync(ct);

    public async Task<UserHabit?> GetActiveUserHabitAsync(string userId, int habitId, CancellationToken ct = default) =>
        await db.UserHabits
            .Include(uh => uh.Habit)
            .FirstOrDefaultAsync(uh => uh.UserId == userId && uh.HabitId == habitId && uh.IsActive, ct);

    public async Task<List<UserHabit>> GetUserHabitsAsync(string userId, CancellationToken ct = default) =>
        await db.UserHabits
            .Include(uh => uh.Habit)
            .Where(uh => uh.UserId == userId && uh.IsActive)
            .ToListAsync(ct);

    public async Task<UserHabit> EnrollAsync(UserHabit userHabit, CancellationToken ct = default)
    {
        db.UserHabits.Add(userHabit);
        await db.SaveChangesAsync(ct);
        await db.Entry(userHabit).Reference(uh => uh.Habit).LoadAsync(ct);
        return userHabit;
    }
}
