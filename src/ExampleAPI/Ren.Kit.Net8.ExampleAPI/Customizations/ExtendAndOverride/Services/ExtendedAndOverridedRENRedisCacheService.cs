using Ren.Kit.CacheKit.Services;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
using StackExchange.Redis;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.ExtendAndOverride.Services;

public class ExtendedAndOverridedRENRedisCacheService(IConnectionMultiplexer connectionMultiplexer) : RENRedisCacheService(connectionMultiplexer), IExtendedRENRedisCacheService
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedAndOverridedRENRedisCacheService AdditionalMethod called.");
        // Implement your additional logic here
    }

    public override void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        Console.WriteLine("ExtendedAndOverridedRENRedisCacheService Set called.");
        // You can add custom logic here before calling the base method
        base.Set(cacheKey, absoluteExpiration, slidingExpiration);
    }
}

