using EnergyCo.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyCo.Infrastructure.Persistence.Configurations;

public sealed class DiscountPromotionProductConfiguration : IEntityTypeConfiguration<DiscountPromotionProduct>
{
    public void Configure(EntityTypeBuilder<DiscountPromotionProduct> builder)
    {
        builder.ToTable("DiscountPromotionProducts");

        builder.HasKey(mapping => new { mapping.DiscountPromotionId, mapping.ProductId });

        builder.Property(mapping => mapping.DiscountPromotionId)
            .HasMaxLength(20);

        builder.Property(mapping => mapping.ProductId)
            .HasMaxLength(20);

        builder.HasIndex(mapping => mapping.ProductId);
    }
}
