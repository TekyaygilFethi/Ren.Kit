using Ren.Kit.CacheKit.Abstractions;
using Ren.Kit.CacheKit.Extensions;
using Ren.Kit.CacheKit.Services;
using StackExchange.Redis;

namespace Ren.Kit.Net8.ExampleAPI.Extensions;

public static class RegisterRENCaching
{
    public enum CacheType
    {
        InMemory,
        Redis
    }

    public static void AddRENCaching(this IServiceCollection services, CacheType cacheType)
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<RENInMemoryCacheService>(services);
                break;
            case CacheType.Redis:
                AddRedisRENCache<RENRedisCacheService>(services);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public static void AddRENCaching<TCacheService>(this IServiceCollection services, CacheType cacheType)
        where TCacheService : IRENCacheService
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<TCacheService>(services);
                break;
            case CacheType.Redis:
                AddRedisRENCache<TCacheService>(services);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public static void AddRENCaching<TICacheService, TCacheService>(this IServiceCollection services, CacheType cacheType)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        switch (cacheType)
        {
            case CacheType.InMemory:
                AddInMemoryRENCache<TICacheService, TCacheService>(services);
                break;
            case CacheType.Redis:
                AddRedisRENCache<TICacheService, TCacheService>(services);
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

    private static void AddRedisRENCache<TCacheService>(this IServiceCollection services)
        where TCacheService : IRENCacheService
    {
        services.GetBaseRedisInjectImplementations();
        services.RegisterRENCacheServices<TCacheService>();
    }

    private static void AddRedisRENCache<TICacheService, TCacheService>(this IServiceCollection services)
        where TICacheService : class, IRENCacheService
        where TCacheService : class, TICacheService
    {
        services.GetBaseRedisInjectImplementations();
        services.RegisterRENCacheServices<TICacheService, TCacheService>();
    }

    private static void GetBaseRedisInjectImplementations(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var redisOptions = new ConfigurationOptions
            {
                EndPoints = { configuration.GetSection("CacheConfiguration:RedisConfiguration:Url")?.Value },
                DefaultDatabase = int.Parse(configuration.GetSection("CacheConfiguration:RedisConfiguration:DatabaseId")?.Value ?? "0"),
                AbortOnConnectFail = bool.Parse(configuration.GetSection("CacheConfiguration:RedisConfiguration:AbortOnConnectFail")?.Value),
                User = configuration.GetSection("CacheConfiguration:RedisConfiguration:Username")?.Value,
                Password = configuration.GetSection("CacheConfiguration:RedisConfiguration:Password")?.Value,
                AllowAdmin = configuration.GetSection("CacheConfiguration:RedisConfiguration:IsAdmin")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false
            };
            return ConnectionMultiplexer.Connect(redisOptions);
        });
    }

}
