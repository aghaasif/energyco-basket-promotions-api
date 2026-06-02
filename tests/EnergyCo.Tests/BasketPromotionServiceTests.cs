using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions;
using EnergyCo.Application.Services.BasketPromotions.Models;
using EnergyCo.Domain.Products;
using EnergyCo.Domain.Promotions;

namespace EnergyCo.Tests;

public sealed class BasketPromotionServiceTests
{
    [Fact]
    public async Task CalculateAsync_returns_totals_without_promotions()
    {
        var service = CreateService();

        var result = await service.CalculateAsync(Command("2020-04-03", Item("PRD01", 1.20m, 3)), CancellationToken.None);

        Assert.Equal(3.60m, result.TotalAmount);
        Assert.Equal(0m, result.Discount.Amount);
        Assert.Equal(3.60m, result.GrandTotal);
        Assert.Equal(0, result.Points.Points);
    }

    [Fact]
    public async Task CalculateAsync_applies_active_category_points_promotion()
    {
        var service = CreateService(pointsPromotions:
        [
            PointsPromotion("PP003", "2020-03-01", "2020-03-20", ProductCategory.Shop, 4)
        ]);

        var result = await service.CalculateAsync(Command("2020-03-10", Item("PRD04", 2.30m, 2)), CancellationToken.None);

        Assert.Equal(4.60m, result.Points.QualifyingAmount);
        Assert.Equal(16, result.Points.Points);
    }

    [Fact]
    public async Task CalculateAsync_applies_discount_only_to_eligible_products()
    {
        var service = CreateService(
            discountPromotions: [DiscountPromotion("DP001", "2020-01-01", "2020-02-15", 20m)],
            mappings: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["DP001"] = ["PRD02"]
            });

        var result = await service.CalculateAsync(
            Command("2020-01-15", Item("PRD01", 1.20m, 3), Item("PRD02", 2.00m, 2)),
            CancellationToken.None);

        Assert.Equal(7.60m, result.TotalAmount);
        Assert.Equal(0.80m, result.Discount.Amount);
        Assert.Equal(6.80m, result.GrandTotal);
    }

    [Fact]
    public async Task CalculateAsync_de_duplicates_discount_product_mappings()
    {
        var service = CreateService(
            discountPromotions: [DiscountPromotion("DP001", "2020-01-01", "2020-02-15", 20m)],
            mappings: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["DP001"] = ["PRD02", "PRD02"]
            });

        var result = await service.CalculateAsync(Command("2020-01-15", Item("PRD02", 2.00m, 2)), CancellationToken.None);

        Assert.Equal(0.80m, result.Discount.Amount);
    }

    [Fact]
    public async Task CalculateAsync_rejects_unknown_products()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BasketPromotionException>(() =>
            service.CalculateAsync(Command("2020-01-15", Item("PRD99", 2.00m, 1)), CancellationToken.None));

        Assert.Contains("PRD99", exception.Message);
    }

    [Fact]
    public async Task CalculateAsync_selects_best_discount_for_customer()
    {
        var service = CreateService(
            discountPromotions:
            [
                DiscountPromotion("DP001", "2020-01-01", "2020-02-15", 10m),
                DiscountPromotion("DP002", "2020-01-01", "2020-02-15", 30m)
            ],
            mappings: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["DP001"] = ["PRD02"],
                ["DP002"] = ["PRD02"]
            });

        var result = await service.CalculateAsync(Command("2020-01-15", Item("PRD02", 2.00m, 2)), CancellationToken.None);

        Assert.Equal("DP002", result.Discount.PromotionId);
        Assert.Equal(1.20m, result.Discount.Amount);
    }

    [Fact]
    public async Task CalculateAsync_applies_best_discount_per_product()
    {
        var service = CreateService(
            discountPromotions:
            [
                DiscountPromotion("DP001", "2020-01-01", "2020-02-15", 20m),
                DiscountPromotion("DP002", "2020-01-01", "2020-02-15", 30m)
            ],
            mappings: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["DP001"] = ["PRD01", "PRD02"],
                ["DP002"] = ["PRD01"]
            });

        var result = await service.CalculateAsync(
            Command("2020-01-15", Item("PRD01", 10.00m, 1), Item("PRD02", 10.00m, 1)),
            CancellationToken.None);

        Assert.Equal(5.00m, result.Discount.Amount);
        Assert.Equal(15.00m, result.GrandTotal);
    }

    [Fact]
    public async Task CalculateAsync_selects_best_points_for_customer()
    {
        var service = CreateService(pointsPromotions:
        [
            PointsPromotion("PP001", "2020-01-01", "2020-01-30", null, 2),
            PointsPromotion("PP002", "2020-01-01", "2020-01-30", ProductCategory.Fuel, 3)
        ]);

        var result = await service.CalculateAsync(Command("2020-01-15", Item("PRD01", 1.20m, 10)), CancellationToken.None);

        Assert.Equal("PP002", result.Points.PromotionId);
        Assert.Equal(36, result.Points.Points);
    }

    [Fact]
    public async Task CalculateAsync_floors_qualifying_spend_before_calculating_points()
    {
        var service = CreateService(pointsPromotions:
        [
            PointsPromotion("PP001", "2020-01-01", "2020-01-30", null, 2)
        ]);

        var result = await service.CalculateAsync(Command("2020-01-15", Item("PRD01", 1.20m, 3)), CancellationToken.None);

        Assert.Equal(3.60m, result.Points.QualifyingAmount);
        Assert.Equal(6, result.Points.Points);
    }

    [Fact]
    public async Task CalculateAsync_supports_post_discount_points_basis()
    {
        var service = CreateService(
            pointsPromotions:
            [
                PointsPromotion("PP001", "2020-01-01", "2020-01-30", null, 2, PointsCalculationBasis.PostDiscount)
            ],
            discountPromotions: [DiscountPromotion("DP001", "2020-01-01", "2020-01-30", 20m)],
            mappings: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["DP001"] = ["PRD02"]
            });

        var result = await service.CalculateAsync(Command("2020-01-15", Item("PRD02", 5.00m, 1)), CancellationToken.None);

        Assert.Equal(1.00m, result.Discount.Amount);
        Assert.Equal(4.00m, result.Points.QualifyingAmount);
        Assert.Equal(8, result.Points.Points);
    }

    private static BasketPromotionService CreateService(
        IReadOnlyCollection<Product>? products = null,
        IReadOnlyCollection<PointsPromotion>? pointsPromotions = null,
        IReadOnlyCollection<DiscountPromotion>? discountPromotions = null,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>>? mappings = null) =>
        new(
            new ProductRepositoryStub(products ?? DefaultProducts()),
            new PointsPromotionRepositoryStub(pointsPromotions ?? []),
            new DiscountPromotionRepositoryStub(discountPromotions ?? [], mappings ?? EmptyMappings()));

    private static BasketPromotionCommand Command(string transactionDate, params BasketPromotionItem[] items) =>
        new(
            Guid.Parse("8e4e8991-aaee-495b-9f24-52d5d0e509c5"),
            "CTX0000001",
            DateTime.SpecifyKind(DateTime.Parse(transactionDate), DateTimeKind.Utc),
            items);

    private static BasketPromotionItem Item(string productId, decimal unitPrice, int quantity) =>
        new(productId, unitPrice, quantity);

    private static Product Product(string productId, ProductCategory category) =>
        new()
        {
            ProductId = productId,
            Name = productId,
            Category = category,
            UnitPrice = 1m
        };

    private static DiscountPromotion DiscountPromotion(
        string promotionId,
        string startDate,
        string endDate,
        decimal discountPercent) =>
        new()
        {
            DiscountPromotionId = promotionId,
            Name = promotionId,
            StartDateUtc = DateTime.SpecifyKind(DateTime.Parse(startDate), DateTimeKind.Utc),
            EndDateUtc = DateTime.SpecifyKind(DateTime.Parse(endDate), DateTimeKind.Utc).AddDays(1),
            DiscountPercent = discountPercent
        };

    private static PointsPromotion PointsPromotion(
        string promotionId,
        string startDate,
        string endDate,
        ProductCategory? category,
        int pointsPerDollar,
        PointsCalculationBasis basis = PointsCalculationBasis.PreDiscount) =>
        new()
        {
            PointsPromotionId = promotionId,
            Name = promotionId,
            StartDateUtc = DateTime.SpecifyKind(DateTime.Parse(startDate), DateTimeKind.Utc),
            EndDateUtc = DateTime.SpecifyKind(DateTime.Parse(endDate), DateTimeKind.Utc).AddDays(1),
            Category = category,
            PointsPerDollar = pointsPerDollar,
            CalculationBasis = basis
        };

    private static IReadOnlyCollection<Product> DefaultProducts() =>
    [
        Product("PRD01", ProductCategory.Fuel),
        Product("PRD02", ProductCategory.Fuel),
        Product("PRD04", ProductCategory.Shop)
    ];

    private static IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyMappings() =>
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);

    private sealed class ProductRepositoryStub(IReadOnlyCollection<Product> products) : IProductRepository
    {
        public Task<IReadOnlyCollection<Product>> GetByIdsAsync(
            IReadOnlyCollection<string> productIds,
            CancellationToken cancellationToken)
        {
            var requested = productIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyCollection<Product>>(
                products.Where(product => requested.Contains(product.ProductId)).ToArray());
        }
    }

    private sealed class PointsPromotionRepositoryStub(
        IReadOnlyCollection<PointsPromotion> pointsPromotions) : IPointsPromotionRepository
    {
        public Task<IReadOnlyCollection<PointsPromotion>> GetActiveAsync(
            DateTime transactionDateUtc,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<PointsPromotion>>(
                pointsPromotions.Where(promotion => promotion.IsActiveOn(transactionDateUtc)).ToArray());
    }

    private sealed class DiscountPromotionRepositoryStub(
        IReadOnlyCollection<DiscountPromotion> discountPromotions,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> mappings) : IDiscountPromotionRepository
    {
        public Task<IReadOnlyCollection<ProductDiscountPromotion>> GetBestActiveProductDiscountsAsync(
            DateTime transactionDateUtc,
            CancellationToken cancellationToken)
        {
            var discounts = discountPromotions
                .Where(promotion => promotion.IsActiveOn(transactionDateUtc))
                .SelectMany(promotion =>
                {
                    var productIds = mappings.TryGetValue(promotion.DiscountPromotionId, out var mappedProductIds)
                        ? mappedProductIds
                        : Array.Empty<string>();

                    return productIds.Select(productId => new ProductDiscountPromotion(
                        productId,
                        promotion.DiscountPromotionId,
                        promotion.Name,
                        promotion.DiscountPercent,
                        promotion.EndDateUtc));
                })
                .GroupBy(discount => discount.ProductId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group
                    .OrderByDescending(discount => discount.DiscountPercent)
                    .ThenBy(discount => discount.EndDateUtc)
                    .ThenBy(discount => discount.DiscountPromotionId, StringComparer.OrdinalIgnoreCase)
                    .First())
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<ProductDiscountPromotion>>(discounts);
        }
    }
}
