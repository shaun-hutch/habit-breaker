using HabitBreaker.API.Models;

namespace HabitBreaker.API.Data.Repositories;

public interface IFoodLogRepository
{
    Task<FoodLog?> GetByDateAsync(int userHabitId, DateOnly date, CancellationToken ct = default);
    Task<FoodLog?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<FoodLog>> GetHistoryAsync(int userHabitId, DateOnly from, DateOnly to, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetHistoryCountAsync(int userHabitId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<FoodLog> UpsertAsync(FoodLog foodLog, CancellationToken ct = default);
    Task DeleteAsync(FoodLog foodLog, CancellationToken ct = default);
}
