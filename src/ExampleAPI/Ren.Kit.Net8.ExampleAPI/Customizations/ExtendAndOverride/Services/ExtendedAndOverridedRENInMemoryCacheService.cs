using Microsoft.Extensions.Caching.Memory;
using Ren.Kit.CacheKit.Services;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.ExtendAndOverride.Services;
public class ExtendedAndOverridedRENInMemoryCacheService(IMemoryCache memoryCache) : RENInMemoryCacheService(memoryCache), IExtendedRENInMemoryCacheService
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedAndOverridedRENInMemoryCacheService AdditionalMethod called.");
        // Implement your additional logic here
    }

    public override void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        Console.WriteLine("ExtendedAndOverridedRENInMemoryCacheService Set called.");
        // You can add custom logic here before calling the base method
        base.Set(cacheKey, absoluteExpiration, slidingExpiration);
    }
}
