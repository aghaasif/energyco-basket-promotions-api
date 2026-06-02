using EnergyCo.Infrastructure.Persistence;
using EnergyCo.Infrastructure.SeedData;
using Microsoft.EntityFrameworkCore;

namespace EnergyCo.Api;

public static class DatabaseStartup
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnergyCoDbContext>();

        await dbContext.Database.MigrateAsync();
        await ReferenceDataSeeder.SeedAsync(dbContext);
    }
}
