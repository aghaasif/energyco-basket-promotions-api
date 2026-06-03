namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record AppliedDiscount(
    decimal EligibleAmount,
    decimal Amount);
