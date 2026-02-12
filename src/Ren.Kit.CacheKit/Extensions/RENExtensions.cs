using Microsoft.Extensions.Configuration;
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
        IConfiguration configuration,
        CacheType cacheType,
        Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null,
        RedisMultiplexerLifetime? multiplexerLifetime = null,
        CacheServiceLifetime cacheServiceLifetime = CacheServiceLifetime.Scoped)
    {
        AddRenOptions(services, configuration);
        services.AddRENCaching(cacheType, implementationFactory, multiplexerLifetime, cacheServiceLifetime);
    }

    public static void AddRENCaching<TCacheService>(
        this IServiceCollection services,
        IConfiguration configuration,
        CacheType cacheType,
        Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null,
        RedisMultiplexerLifetime? multiplexerLifetime = null,
        CacheServiceLifetime cacheServiceLifetime = CacheServiceLifetime.Scoped)
        where TCacheService : class, IRENCacheService
    {
        AddRenOptions(services, configuration);
        services.AddRENCaching<TCacheService>(cacheType, implementationFactory, multiplexerLifetime, cacheServiceLifetime);
    }

    public static void AddRENCaching<TICacheService, TCacheService>(
        this IServiceCollection services,
        IConfiguration configuration,
        CacheType cacheType,
        Func<IServiceProvider, IConnectionMultiplexer>? implementationFactory = null,
        RedisMultiplexerLifetime? multiplexerLifetime = null,
        CacheServiceLifetime cacheServiceLifetime = CacheServiceLifetime.Scoped)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        AddRenOptions(services, configuration);
        services.AddRENCaching<TICacheService, TCacheService>(cacheType, implementationFactory, multiplexerLifetime, cacheServiceLifetime);
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
                    implementationFactory ?? throw new ArgumentNullException(nameof(implementationFactory)),
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
                    implementationFactory ?? throw new ArgumentNullException(nameof(implementationFactory)),
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
                    implementationFactory ?? throw new ArgumentNullException(nameof(implementationFactory)),
                    multiplexerLifetime ?? RedisMultiplexerLifetime.Singleton,
                    cacheServiceLifetime);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }
    private static void AddRenOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RENCacheKitOptions>(options =>
        {
            configuration
                .GetSection("CacheConfiguration")
                .Bind(options.CacheConfiguration);
        });
    }

    private static void AddInMemoryRENCache<TCacheService>(
        IServiceCollection services,
        CacheServiceLifetime lifetime)
        where TCacheService : class, IRENCacheService
    {
        services.AddMemoryCache();
        services.RegisterRENCacheServices<TCacheService>(lifetime);
    }

    private static void AddInMemoryRENCache<TICacheService, TCacheService>(
        IServiceCollection services,
        CacheServiceLifetime lifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.AddMemoryCache();
        services.RegisterRENCacheServices<TICacheService, TCacheService>(lifetime);
    }

    private static void AddRedisRENCache<TCacheService>(
        IServiceCollection services,
        Func<IServiceProvider, IConnectionMultiplexer> factory,
        RedisMultiplexerLifetime multiplexerLifetime,
        CacheServiceLifetime lifetime)
        where TCacheService : class, IRENCacheService
    {
        services.GetBaseRedisInjectImplementations(factory, multiplexerLifetime);
        services.RegisterRENCacheServices<TCacheService>(lifetime);
    }

    private static void AddRedisRENCache<TICacheService, TCacheService>(
        IServiceCollection services,
        Func<IServiceProvider, IConnectionMultiplexer> factory,
        RedisMultiplexerLifetime multiplexerLifetime,
        CacheServiceLifetime lifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.GetBaseRedisInjectImplementations(factory, multiplexerLifetime);
        services.RegisterRENCacheServices<TICacheService, TCacheService>(lifetime);
    }

    private static void GetBaseRedisInjectImplementations(
        this IServiceCollection services,
        Func<IServiceProvider, IConnectionMultiplexer> factory,
        RedisMultiplexerLifetime lifetime)
    {
        switch (lifetime)
        {
            case RedisMultiplexerLifetime.Singleton:
                services.AddSingleton(factory);
                break;
            case RedisMultiplexerLifetime.Scoped:
                services.AddScoped(factory);
                break;
            case RedisMultiplexerLifetime.Transient:
                services.AddTransient(factory);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    private static IServiceCollection RegisterRENCacheServices<TCacheService>(
        this IServiceCollection services,
        CacheServiceLifetime lifetime)
        where TCacheService : class, IRENCacheService
    {
        switch (lifetime)
        {
            case CacheServiceLifetime.Singleton:
                services.AddSingleton(typeof(IRENCacheService), typeof(TCacheService));
                break;
            case CacheServiceLifetime.Scoped:
                services.AddScoped(typeof(IRENCacheService), typeof(TCacheService));
                break;
            case CacheServiceLifetime.Transient:
                services.AddTransient(typeof(IRENCacheService), typeof(TCacheService));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        return services;
    }

    private static IServiceCollection RegisterRENCacheServices<TICacheService, TCacheService>(
        this IServiceCollection services,
        CacheServiceLifetime lifetime)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        switch (lifetime)
        {
            case CacheServiceLifetime.Singleton:
                services.AddSingleton(typeof(TICacheService), typeof(TCacheService));
                break;
            case CacheServiceLifetime.Scoped:
                services.AddScoped(typeof(TICacheService), typeof(TCacheService));
                break;
            case CacheServiceLifetime.Transient:
                services.AddTransient(typeof(TICacheService), typeof(TCacheService));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        return services;
    }
}
