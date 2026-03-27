namespace HabitBreaker.API.Models;

public class Habit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefaultDurationDays { get; set; } = 66;

    public ICollection<UserHabit> UserHabits { get; set; } = [];
}
