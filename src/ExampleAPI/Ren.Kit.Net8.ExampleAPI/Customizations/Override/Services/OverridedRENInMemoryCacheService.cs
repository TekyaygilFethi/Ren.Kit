using Microsoft.Extensions.Caching.Memory;
using Ren.Kit.CacheKit.Services;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Override.Services;

public class OverridedRENInMemoryCacheService(IMemoryCache memoryCache) : RENInMemoryCacheService(memoryCache)
{
    public override async Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"OverridedRENInMemoryCacheService GetAsync called");
        // You can add custom logic here before calling the base method
        return await base.GetAsync<T>(cacheKey, cancellationToken);
    }
}
