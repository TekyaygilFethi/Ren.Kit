using Microsoft.Extensions.DependencyInjection;
using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.CacheKit.Extensions;

public static class RENExtensions
{
    public static IServiceCollection RegisterRENCacheServices<TCacheService>(this IServiceCollection services)
        where TCacheService : IRENCacheService
    {
        services.AddScoped(typeof(IRENCacheService), typeof(TCacheService));

        return services;
    }

    public static IServiceCollection RegisterRENCacheServices<TICacheService, TCacheService>(this IServiceCollection services)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.AddScoped(typeof(TICacheService), typeof(TCacheService));

        return services;
    }
}
