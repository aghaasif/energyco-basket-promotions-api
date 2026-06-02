using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnergyCo.Infrastructure.Persistence;

public sealed class EnergyCoDbContextDesignTimeFactory : IDesignTimeDbContextFactory<EnergyCoDbContext>
{
    public EnergyCoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EnergyCoDbContext>()
            .UseSqlite("Data Source=energyco.db")
            .Options;

        return new EnergyCoDbContext(options);
    }
}
