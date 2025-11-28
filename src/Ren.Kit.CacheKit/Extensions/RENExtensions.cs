using Microsoft.Extensions.DependencyInjection;
using Ren.Kit.CacheKit.Abstractions;
using Ren.Kit.CacheKit.Services;
using StackExchange.Redis;

namespace Ren.Kit.CacheKit.Extensions;

public static class RENExtensions
{
    public enum CacheType
    {
        InMemory,
        Redis
    }

    public enum RedisMultiplexerLifetime
    {
        Singleton,
        Scoped,
        Transient
    }

    public static void AddRENCaching(this IServiceCollection services, CacheType cacheType, Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null, RedisMultiplexerLifetime? multiplexerLifetime = null)
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<RENInMemoryCacheService>(services);
                break;
            case CacheType.Redis:
                AddRedisRENCache<RENRedisCacheService>(services, implementationFactory, multiplexerLifetime.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public static void AddRENCaching<TCacheService>(this IServiceCollection services, CacheType cacheType, Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null, RedisMultiplexerLifetime? multiplexerLifetime = null)
        where TCacheService : IRENCacheService
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<TCacheService>(services);
                break;
            case CacheType.Redis:
                AddRedisRENCache<TCacheService>(services, implementationFactory, multiplexerLifetime.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public static void AddRENCaching<TICacheService, TCacheService>(this IServiceCollection services, CacheType cacheType, Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null, RedisMultiplexerLifetime? multiplexerLifetime = null)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<TICacheService, TCacheService>(services);
                break;
            case CacheType.Redis:
                AddRedisRENCache<TICacheService, TCacheService>(services, implementationFactory, multiplexerLifetime.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    private static void AddInMemoryRENCache<TCacheService>(this IServiceCollection services)
        where TCacheService : IRENCacheService
    {
        services.AddMemoryCache();
        services.RegisterRENCacheServices<TCacheService>();
    }

    private static void AddInMemoryRENCache<TICacheService, TCacheService>(this IServiceCollection services)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.AddMemoryCache();
        services.RegisterRENCacheServices<TICacheService, TCacheService>();
    }

    private static void AddRedisRENCache<TCacheService>(this IServiceCollection services, Func<IServiceProvider, IConnectionMultiplexer> implementationFactory, RedisMultiplexerLifetime multiplexerLifetime)
        where TCacheService : IRENCacheService
    {
        services.GetBaseRedisInjectImplementations(implementationFactory, multiplexerLifetime);
        services.RegisterRENCacheServices<TCacheService>();
    }

    private static void AddRedisRENCache<TICacheService, TCacheService>(this IServiceCollection services, Func<IServiceProvider, IConnectionMultiplexer> implementationFactory, RedisMultiplexerLifetime multiplexerLifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.GetBaseRedisInjectImplementations(implementationFactory, multiplexerLifetime);
        services.RegisterRENCacheServices<TICacheService, TCacheService>();
    }

    private static void GetBaseRedisInjectImplementations(this IServiceCollection services, Func<IServiceProvider, IConnectionMultiplexer> implementationFactory, RedisMultiplexerLifetime multiplexerRuntime)
    {
        if (implementationFactory == null)
            throw new ArgumentNullException(nameof(implementationFactory));

        if (multiplexerRuntime == RedisMultiplexerLifetime.Singleton)
            services.AddSingleton<IConnectionMultiplexer>(implementationFactory);
        else if (multiplexerRuntime == RedisMultiplexerLifetime.Scoped)
            services.AddScoped<IConnectionMultiplexer>(implementationFactory);
        else if (multiplexerRuntime == RedisMultiplexerLifetime.Transient)
            services.AddTransient<IConnectionMultiplexer>(implementationFactory);
        else
            throw new ArgumentOutOfRangeException(nameof(multiplexerRuntime), multiplexerRuntime, null);
    }

    private static IServiceCollection RegisterRENCacheServices<TCacheService>(this IServiceCollection services)
        where TCacheService : IRENCacheService
    {
        services.AddScoped(typeof(IRENCacheService), typeof(TCacheService));

        return services;
    }

    private static IServiceCollection RegisterRENCacheServices<TICacheService, TCacheService>(this IServiceCollection services)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.AddScoped(typeof(TICacheService), typeof(TCacheService));

        return services;
    }
}
