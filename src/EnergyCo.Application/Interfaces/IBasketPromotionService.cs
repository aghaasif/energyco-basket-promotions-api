using EnergyCo.Application.Services.BasketPromotions.Models;

namespace EnergyCo.Application.Interfaces;

public interface IBasketPromotionService
{
    Task<BasketPromotionResult> CalculateAsync(
        BasketPromotionCommand command,
        CancellationToken cancellationToken);
}
