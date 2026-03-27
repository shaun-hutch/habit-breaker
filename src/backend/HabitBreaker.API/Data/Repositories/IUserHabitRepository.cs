using HabitBreaker.API.Models;

namespace HabitBreaker.API.Data.Repositories;

public interface IUserHabitRepository
{
    Task<List<Habit>> GetAllHabitsAsync(CancellationToken ct = default);
    Task<UserHabit?> GetActiveUserHabitAsync(string userId, int habitId, CancellationToken ct = default);
    Task<List<UserHabit>> GetUserHabitsAsync(string userId, CancellationToken ct = default);
    Task<UserHabit> EnrollAsync(UserHabit userHabit, CancellationToken ct = default);
}
