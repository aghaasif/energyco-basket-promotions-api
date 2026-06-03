using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions;
using EnergyCo.Application.Services.BasketPromotions.Calculators;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyCo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBasketPromotionService, BasketPromotionService>();
        services.AddSingleton<DiscountCalculator>();
        services.AddSingleton<PointsCalculator>();

        return services;
    }
}
