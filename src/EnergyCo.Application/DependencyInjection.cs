using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyCo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBasketPromotionService, BasketPromotionService>();

        return services;
    }
}
