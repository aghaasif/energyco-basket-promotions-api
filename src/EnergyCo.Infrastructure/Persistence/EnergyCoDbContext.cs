using EnergyCo.Domain.Products;
using EnergyCo.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace EnergyCo.Infrastructure.Persistence;

public sealed class EnergyCoDbContext(DbContextOptions<EnergyCoDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<PointsPromotion> PointsPromotions => Set<PointsPromotion>();

    public DbSet<DiscountPromotion> DiscountPromotions => Set<DiscountPromotion>();

    public DbSet<DiscountPromotionProduct> DiscountPromotionProducts => Set<DiscountPromotionProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnergyCoDbContext).Assembly);
    }
}
