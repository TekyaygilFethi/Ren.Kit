using Ren.Kit.CacheKit.Services;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;
using StackExchange.Redis;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Services;

public class ExtendedRENRedisCacheService(IConnectionMultiplexer connectionMultiplexer) : RENRedisCacheService(connectionMultiplexer), IExtendedRENRedisCacheService
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedRENRedisCacheService AdditionalMethod called.");
        // Implement your additional logic here
    }
}

