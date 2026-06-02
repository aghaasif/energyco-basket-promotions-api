using EnergyCo.Application.Interfaces;
using EnergyCo.Infrastructure.Persistence;
using EnergyCo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyCo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=energyco.db";

        services.AddDbContext<EnergyCoDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPointsPromotionRepository, PointsPromotionRepository>();
        services.AddScoped<IDiscountPromotionRepository, DiscountPromotionRepository>();

        return services;
    }
}
