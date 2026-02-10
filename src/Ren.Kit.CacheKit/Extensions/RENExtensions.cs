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

    public enum CacheServiceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }

    public static void AddRENCaching(
        this IServiceCollection services,
        CacheType cacheType,
        Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null,
        RedisMultiplexerLifetime? multiplexerLifetime = null,
        CacheServiceLifetime cacheServiceLifetime = CacheServiceLifetime.Scoped)
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<RENInMemoryCacheService>(services, cacheServiceLifetime);
                break;

            case CacheType.Redis:
                AddRedisRENCache<RENRedisCacheService>(
                    services,
                    implementationFactory!,
                    multiplexerLifetime ?? RedisMultiplexerLifetime.Singleton,
                    cacheServiceLifetime);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public static void AddRENCaching<TCacheService>(
        this IServiceCollection services,
        CacheType cacheType,
        Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null,
        RedisMultiplexerLifetime? multiplexerLifetime = null,
        CacheServiceLifetime cacheServiceLifetime = CacheServiceLifetime.Scoped)
        where TCacheService : class, IRENCacheService
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<TCacheService>(services, cacheServiceLifetime);
                break;

            case CacheType.Redis:
                AddRedisRENCache<TCacheService>(
                    services,
                    implementationFactory!,
                    multiplexerLifetime ?? RedisMultiplexerLifetime.Singleton,
                    cacheServiceLifetime);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public static void AddRENCaching<TICacheService, TCacheService>(
        this IServiceCollection services,
        CacheType cacheType,
        Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null,
        RedisMultiplexerLifetime? multiplexerLifetime = null,
        CacheServiceLifetime cacheServiceLifetime = CacheServiceLifetime.Scoped)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<TICacheService, TCacheService>(services, cacheServiceLifetime);
                break;

            case CacheType.Redis:
                AddRedisRENCache<TICacheService, TCacheService>(
                    services,
                    implementationFactory!,
                    multiplexerLifetime ?? RedisMultiplexerLifetime.Singleton,
                    cacheServiceLifetime);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    private static void AddInMemoryRENCache<TCacheService>(
        IServiceCollection services,
        CacheServiceLifetime cacheServiceLifetime)
        where TCacheService : class, IRENCacheService
    {
        services.AddMemoryCache();
        services.RegisterRENCacheServices<TCacheService>(cacheServiceLifetime);
    }

    private static void AddInMemoryRENCache<TICacheService, TCacheService>(
        IServiceCollection services,
        CacheServiceLifetime cacheServiceLifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.AddMemoryCache();
        services.RegisterRENCacheServices<TICacheService, TCacheService>(cacheServiceLifetime);
    }

    private static void AddRedisRENCache<TCacheService>(
        IServiceCollection services,
        Func<IServiceProvider, IConnectionMultiplexer> implementationFactory,
        RedisMultiplexerLifetime multiplexerLifetime,
        CacheServiceLifetime cacheServiceLifetime)
        where TCacheService : class, IRENCacheService
    {
        services.GetBaseRedisInjectImplementations(implementationFactory, multiplexerLifetime);
        services.RegisterRENCacheServices<TCacheService>(cacheServiceLifetime);
    }

    private static void AddRedisRENCache<TICacheService, TCacheService>(
        IServiceCollection services,
        Func<IServiceProvider, IConnectionMultiplexer> implementationFactory,
        RedisMultiplexerLifetime multiplexerLifetime,
        CacheServiceLifetime cacheServiceLifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.GetBaseRedisInjectImplementations(implementationFactory, multiplexerLifetime);
        services.RegisterRENCacheServices<TICacheService, TCacheService>(cacheServiceLifetime);
    }

    private static void GetBaseRedisInjectImplementations(
        this IServiceCollection services,
        Func<IServiceProvider, IConnectionMultiplexer> implementationFactory,
        RedisMultiplexerLifetime multiplexerRuntime)
    {
        if (multiplexerRuntime == RedisMultiplexerLifetime.Singleton)
            services.AddSingleton(implementationFactory);
        else if (multiplexerRuntime == RedisMultiplexerLifetime.Scoped)
            services.AddScoped(implementationFactory);
        else if (multiplexerRuntime == RedisMultiplexerLifetime.Transient)
            services.AddTransient(implementationFactory);
        else
            throw new ArgumentOutOfRangeException(nameof(multiplexerRuntime), multiplexerRuntime, null);
    }

    private static IServiceCollection RegisterRENCacheServices<TCacheService>(
        this IServiceCollection services,
        CacheServiceLifetime cacheServiceLifetime)
        where TCacheService : class, IRENCacheService
    {
        if (cacheServiceLifetime == CacheServiceLifetime.Singleton)
            services.AddSingleton(typeof(IRENCacheService), typeof(TCacheService));
        else if (cacheServiceLifetime == CacheServiceLifetime.Scoped)
            services.AddScoped(typeof(IRENCacheService), typeof(TCacheService));
        else if (cacheServiceLifetime == CacheServiceLifetime.Transient)
            services.AddTransient(typeof(IRENCacheService), typeof(TCacheService));
        else
            throw new ArgumentOutOfRangeException(nameof(cacheServiceLifetime), cacheServiceLifetime, null);

        return services;
    }

    private static IServiceCollection RegisterRENCacheServices<TICacheService, TCacheService>(
        this IServiceCollection services,
        CacheServiceLifetime cacheServiceLifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        if (cacheServiceLifetime == CacheServiceLifetime.Singleton)
            services.AddSingleton(typeof(TICacheService), typeof(TCacheService));
        else if (cacheServiceLifetime == CacheServiceLifetime.Scoped)
            services.AddScoped(typeof(TICacheService), typeof(TCacheService));
        else if (cacheServiceLifetime == CacheServiceLifetime.Transient)
            services.AddTransient(typeof(TICacheService), typeof(TCacheService));
        else
            throw new ArgumentOutOfRangeException(nameof(cacheServiceLifetime), cacheServiceLifetime, null);

        return services;
    }
}
