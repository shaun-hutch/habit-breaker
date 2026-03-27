using HabitBreaker.API.Data;
using HabitBreaker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitBreaker.API.Jobs;

public class PushReminderJob(
    ApplicationDbContext db,
    Services.IPushNotificationService pushService,
    ILogger<PushReminderJob> logger)
{
    public async Task ExecuteAsync()
    {
        var nowUtc = TimeOnly.FromDateTime(DateTime.UtcNow);
        // Match users whose reminder time is within the current minute window
        var windowStart = nowUtc.AddMinutes(-1);
        var windowEnd = nowUtc;

        var users = await db.Users
            .Where(u => u.PreferredReminderTime.HasValue
                && u.PreferredReminderTime.Value >= windowStart
                && u.PreferredReminderTime.Value <= windowEnd)
            .ToListAsync();

        logger.LogInformation("PushReminderJob: found {Count} users to notify", users.Count);

        foreach (var user in users)
        {
            await pushService.SendReminderToUserAsync(user);
        }
    }
}
