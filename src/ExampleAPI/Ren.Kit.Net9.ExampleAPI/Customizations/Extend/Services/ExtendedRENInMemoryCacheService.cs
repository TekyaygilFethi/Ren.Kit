using Microsoft.Extensions.Caching.Memory;
using Ren.Kit.CacheKit.Services;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Services;

public class ExtendedRENInMemoryCacheService(IMemoryCache memoryCache) : RENInMemoryCacheService(memoryCache), IExtendedRENInMemoryCacheService
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedRENInMemoryCacheService AdditionalMethod called.");
        // Implement your additional logic here
    }
}

