namespace HabitBreaker.API.Models;

public class UserHabit
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int HabitId { get; set; }
    public DateOnly StartDate { get; set; }
    public int DurationDays { get; set; } = 66;
    public bool IsActive { get; set; } = true;

    public ApplicationUser User { get; set; } = null!;
    public Habit Habit { get; set; } = null!;
    public ICollection<FoodLog> FoodLogs { get; set; } = [];
}
