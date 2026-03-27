using HabitBreaker.API.Data;
using HabitBreaker.API.Models;
using HabitBreaker.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;
using WPSubscription = WebPush.PushSubscription;

namespace HabitBreaker.API.Services;

public interface IPushNotificationService
{
    Task SendReminderToUserAsync(ApplicationUser user, CancellationToken ct = default);
    Task SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth, CancellationToken ct = default);
    Task RemoveSubscriptionAsync(string userId, string endpoint, CancellationToken ct = default);
}

public class PushNotificationService(
    ApplicationDbContext db,
    IOptions<VapidSettings> vapidOptions,
    ILogger<PushNotificationService> logger) : IPushNotificationService
{
    private readonly VapidSettings _vapid = vapidOptions.Value;

    public async Task SendReminderToUserAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var subscriptions = await db.PushSubscriptions
            .Where(ps => ps.UserId == user.Id)
            .ToListAsync(ct);

        if (subscriptions.Count == 0) return;

        var client = new WebPushClient();
        client.SetVapidDetails(_vapid.Subject, _vapid.PublicKey, _vapid.PrivateKey);

        var payload = """{"title":"Habit Reminder","body":"Don't forget to log your habits today!","icon":"/icons/icon-192x192.png"}""";

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSub = new WPSubscription { Endpoint = sub.Endpoint, P256DH = sub.P256dhKey, Auth = sub.AuthKey };
                await client.SendNotificationAsync(pushSub, payload);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                // Subscription expired/unsubscribed — clean it up
                db.PushSubscriptions.Remove(sub);
                logger.LogInformation("Removed expired push subscription for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send push notification to user {UserId}", user.Id);
            }
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth, CancellationToken ct = default)
    {
        var existing = await db.PushSubscriptions
            .FirstOrDefaultAsync(ps => ps.UserId == userId && ps.Endpoint == endpoint, ct);
        if (existing is null)
        {
            db.PushSubscriptions.Add(new Models.PushSubscription
            {
                UserId = userId,
                Endpoint = endpoint,
                P256dhKey = p256dh,
                AuthKey = auth
            });
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveSubscriptionAsync(string userId, string endpoint, CancellationToken ct = default)
    {
        var sub = await db.PushSubscriptions
            .FirstOrDefaultAsync(ps => ps.UserId == userId && ps.Endpoint == endpoint, ct);
        if (sub is not null)
        {
            db.PushSubscriptions.Remove(sub);
            await db.SaveChangesAsync(ct);
        }
    }
}
