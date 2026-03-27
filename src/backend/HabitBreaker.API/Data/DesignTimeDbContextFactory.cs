using HabitBreaker.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HabitBreaker.API;

/// <summary>
/// Design-time factory so EF CLI can create the DbContext without needing a running database.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=HabitBreaker;User Id=sa;Password=Dev!Password1;TrustServerCertificate=True;");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
