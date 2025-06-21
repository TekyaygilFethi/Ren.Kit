using Microsoft.Extensions.DependencyInjection;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.DataKit.Services;

namespace Ren.Kit.DataKit.Extensions;

public static class RENExtensions
{
    public static IServiceCollection RegisterRENDataServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRENUnitOfWork<>), typeof(RENUnitOfWork<>));

        return services;
    }

    public static IServiceCollection RegisterRENDataServices(this IServiceCollection services, Type unitOfWorkOpenType)
    {
        services.AddScoped(typeof(IRENUnitOfWork<>), unitOfWorkOpenType);

        return services;
    }

    public static IServiceCollection RegisterRENDataServices<TIRENUnitOfWork, TRENUnitOfWork>(this IServiceCollection services)
    {
        services.AddScoped(typeof(TIRENUnitOfWork), typeof(TRENUnitOfWork));

        return services;
    }
}
