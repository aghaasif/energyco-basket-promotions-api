using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnergyCo.Infrastructure.Persistence;

public sealed class EnergyCoDbContextDesignTimeFactory : IDesignTimeDbContextFactory<EnergyCoDbContext>
{
    public EnergyCoDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Data Source=energyco.db";

        var options = new DbContextOptionsBuilder<EnergyCoDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new EnergyCoDbContext(options);
    }
}
