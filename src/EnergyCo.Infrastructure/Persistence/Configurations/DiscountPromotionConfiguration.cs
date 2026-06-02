using EnergyCo.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyCo.Infrastructure.Persistence.Configurations;

public sealed class DiscountPromotionConfiguration : IEntityTypeConfiguration<DiscountPromotion>
{
    public void Configure(EntityTypeBuilder<DiscountPromotion> builder)
    {
        builder.ToTable("DiscountPromotions");

        builder.HasKey(promotion => promotion.DiscountPromotionId);

        builder.Property(promotion => promotion.DiscountPromotionId)
            .HasMaxLength(20);

        builder.Property(promotion => promotion.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(promotion => promotion.DiscountPercent)
            .HasColumnType("decimal(5,2)");

        builder.HasIndex(promotion => new { promotion.StartDate, promotion.EndDate });
    }
}
