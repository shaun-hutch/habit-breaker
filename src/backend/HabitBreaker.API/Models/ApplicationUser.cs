using Microsoft.AspNetCore.Identity;

namespace HabitBreaker.API.Models;

public class ApplicationUser : IdentityUser
{
    public TimeOnly? PreferredReminderTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserHabit> UserHabits { get; set; } = [];
    public ICollection<PushSubscription> PushSubscriptions { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
