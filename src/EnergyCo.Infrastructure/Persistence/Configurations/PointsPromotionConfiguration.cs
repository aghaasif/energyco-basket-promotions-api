using EnergyCo.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyCo.Infrastructure.Persistence.Configurations;

public sealed class PointsPromotionConfiguration : IEntityTypeConfiguration<PointsPromotion>
{
    public void Configure(EntityTypeBuilder<PointsPromotion> builder)
    {
        builder.ToTable("PointsPromotions");

        builder.HasKey(promotion => promotion.PointsPromotionId);

        builder.Property(promotion => promotion.PointsPromotionId)
            .HasMaxLength(20);

        builder.Property(promotion => promotion.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(promotion => promotion.Category)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(promotion => promotion.CalculationBasis)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(promotion => new { promotion.StartDate, promotion.EndDate, promotion.Category });
    }
}
