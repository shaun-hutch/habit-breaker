using System.ComponentModel.DataAnnotations;

namespace HabitBreaker.API.Dtos;

// ── Auth ─────────────────────────────────────────────────────────────────────

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(256)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshRequest([Required] string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);

// ── User ─────────────────────────────────────────────────────────────────────

public record UserDto(string Id, string Email, string? PreferredReminderTime);

// ── Habits ───────────────────────────────────────────────────────────────────

public record HabitDto(int Id, string Name, string Description, int DefaultDurationDays);

public record EnrollRequest([Required] int HabitId, int? DurationDays);

public record UserHabitDto(
    int Id,
    HabitDto Habit,
    string StartDate,
    int DurationDays,
    int DayNumber,
    int TotalDays,
    double ProgressPercent,
    bool IsComplete);

// ── Food Log ─────────────────────────────────────────────────────────────────

public record MealEntryRequest(
    [Required] int MealType,
    [Required, MaxLength(2000)] string Description,
    [Required] int Rating);

public record UpsertFoodLogRequest(
    [Required] string LogDate,
    [Required] int UserHabitId,
    IList<MealEntryRequest> MealEntries);

public record MealEntryDto(int Id, int MealType, string MealTypeName, string Description, int Rating, string RatingName);

public record FoodLogDto(int Id, int UserHabitId, string LogDate, DateTime CreatedAt, DateTime UpdatedAt, IList<MealEntryDto> MealEntries);

public record FoodLogHistoryResponse(IList<FoodLogDto> Items, int TotalCount, int Page, int PageSize);

// ── Push ─────────────────────────────────────────────────────────────────────

public record PushSubscribeRequest(
    [Required] string Endpoint,
    [Required] string P256dh,
    [Required] string Auth);

public record PushUnsubscribeRequest([Required] string Endpoint);

public record UpdateReminderTimeRequest(string? ReminderTime); // "HH:mm" or null to clear
