using System.ComponentModel.DataAnnotations;

namespace EnergyCo.Api.V1.Contracts;

public sealed record BasketPromotionRequest(
    [property: Required]
    Guid CustomerId,

    [property: Required]
    [property: MinLength(1)]
    string LoyaltyCard,

    [property: Required]
    DateOnly TransactionDate,

    [property: Required]
    [property: MinLength(1)]
    IReadOnlyList<BasketItemRequest> Basket);
