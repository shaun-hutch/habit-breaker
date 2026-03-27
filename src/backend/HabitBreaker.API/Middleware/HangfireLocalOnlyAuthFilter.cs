using Hangfire.Dashboard;

namespace HabitBreaker.API.Middleware;

/// <summary>
/// Restricts Hangfire dashboard to localhost requests only.
/// </summary>
public class HangfireLocalOnlyAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        return remoteIp is not null
            && (remoteIp.Equals(System.Net.IPAddress.Loopback)
                || remoteIp.Equals(System.Net.IPAddress.IPv6Loopback)
                || remoteIp.ToString() == "::1");
    }
}
