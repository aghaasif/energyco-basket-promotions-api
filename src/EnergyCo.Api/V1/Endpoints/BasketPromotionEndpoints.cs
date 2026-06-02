using Asp.Versioning;
using EnergyCo.Api.V1.Contracts;
using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions;
using EnergyCo.Application.Services.BasketPromotions.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EnergyCo.Api.V1.Endpoints;

public static class BasketPromotionEndpoints
{
    public static RouteGroupBuilder MapBasketPromotionEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/basket-promotions", CalculateBasketPromotionsAsync)
            .MapToApiVersion(new ApiVersion(1, 0))
            .RequireRateLimiting("basket-promotions")
            .WithName("CalculateBasketPromotions")
            .WithTags("Basket Promotions")
            .WithSummary("Calculates applicable basket discounts and loyalty points.");

        return group;
    }

    private static async Task<Results<Ok<BasketPromotionResponse>, ProblemHttpResult>> CalculateBasketPromotionsAsync(
        BasketPromotionRequest request,
        IBasketPromotionService basketPromotionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await basketPromotionService.CalculateAsync(request.ToCommand(), cancellationToken);
            return TypedResults.Ok(result.ToResponse());
        }
        catch (BasketPromotionException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Basket promotion calculation failed");
        }
    }

    private static BasketPromotionCommand ToCommand(this BasketPromotionRequest request) =>
        new(
            request.CustomerId,
            request.LoyaltyCard,
            request.TransactionDate,
            request.Basket
                .Select(item => new BasketPromotionItem(item.ProductId, item.UnitPrice, item.Quantity))
                .ToArray());

    private static BasketPromotionResponse ToResponse(this BasketPromotionResult result) =>
        new(
            result.CustomerId,
            result.LoyaltyCard,
            result.TransactionDate,
            FormatMoney(result.TotalAmount),
            FormatMoney(result.Discount.Amount),
            FormatMoney(result.GrandTotal),
            result.Points.Points.ToString("0"));

    private static string FormatMoney(decimal amount) => amount.ToString("0.00");
}
