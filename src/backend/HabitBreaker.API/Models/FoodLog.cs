namespace HabitBreaker.API.Models;

public enum MealType
{
    Breakfast = 0,
    MorningTea = 1,
    Lunch = 2,
    Dinner = 3,
    Supper = 4
}

public enum MealRating
{
    Healthy = 0,
    Unhealthy = 1
}

public class FoodLog
{
    public int Id { get; set; }
    public int UserHabitId { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public UserHabit UserHabit { get; set; } = null!;
    public ICollection<MealEntry> MealEntries { get; set; } = [];
}

public class MealEntry
{
    public int Id { get; set; }
    public int FoodLogId { get; set; }
    public MealType MealType { get; set; }
    public string Description { get; set; } = string.Empty;
    public MealRating Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FoodLog FoodLog { get; set; } = null!;
}
