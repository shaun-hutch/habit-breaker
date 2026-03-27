using HabitBreaker.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HabitBreaker.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<UserHabit> UserHabits => Set<UserHabit>();
    public DbSet<FoodLog> FoodLogs => Set<FoodLog>();
    public DbSet<MealEntry> MealEntries => Set<MealEntry>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.PreferredReminderTime)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString("HH:mm:ss") : null,
                    v => v != null ? TimeOnly.Parse(v) : (TimeOnly?)null);
        });

        builder.Entity<Habit>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Name).HasMaxLength(200).IsRequired();
            e.Property(h => h.Description).HasMaxLength(2000);
            // Seed the initial "Better Eating" habit
            e.HasData(new Habit
            {
                Id = 1,
                Name = "Better Eating",
                Description = "Track your daily food and drink intake to build healthier eating habits.",
                DefaultDurationDays = 66
            });
        });

        builder.Entity<UserHabit>(e =>
        {
            e.HasKey(uh => uh.Id);
            e.HasOne(uh => uh.User).WithMany(u => u.UserHabits).HasForeignKey(uh => uh.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(uh => uh.Habit).WithMany(h => h.UserHabits).HasForeignKey(uh => uh.HabitId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(uh => new { uh.UserId, uh.HabitId, uh.IsActive });
        });

        builder.Entity<FoodLog>(e =>
        {
            e.HasKey(fl => fl.Id);
            e.HasOne(fl => fl.UserHabit).WithMany(uh => uh.FoodLogs).HasForeignKey(fl => fl.UserHabitId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(fl => new { fl.UserHabitId, fl.LogDate }).IsUnique();
        });

        builder.Entity<MealEntry>(e =>
        {
            e.HasKey(me => me.Id);
            e.HasOne(me => me.FoodLog).WithMany(fl => fl.MealEntries).HasForeignKey(me => me.FoodLogId).OnDelete(DeleteBehavior.Cascade);
            e.Property(me => me.Description).HasMaxLength(2000);
        });

        builder.Entity<PushSubscription>(e =>
        {
            e.HasKey(ps => ps.Id);
            e.HasOne(ps => ps.User).WithMany(u => u.PushSubscriptions).HasForeignKey(ps => ps.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(ps => ps.Endpoint).HasMaxLength(1000);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(rt => rt.Token).HasMaxLength(500);
            e.HasIndex(rt => rt.Token).IsUnique();
        });
    }
}
