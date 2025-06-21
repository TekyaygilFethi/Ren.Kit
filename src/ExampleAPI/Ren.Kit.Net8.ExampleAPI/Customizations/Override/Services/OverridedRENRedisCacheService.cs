using Ren.Kit.CacheKit.Services;
using StackExchange.Redis;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Override.Services;

public class OverridedRENRedisCacheService(IConnectionMultiplexer connectionMultiplexer) : RENRedisCacheService(connectionMultiplexer)
{
    public override async Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"OverridedRENRedisCacheService GetAsync called with cacheKey: {cacheKey}");
        // You can add custom logic here before calling the base method
        return await base.GetAsync<T>(cacheKey, cancellationToken);
    }
}