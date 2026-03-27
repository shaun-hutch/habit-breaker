namespace HabitBreaker.API.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "HabitBreaker";
    public string Audience { get; set; } = "HabitBreakerUsers";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 30;
}
