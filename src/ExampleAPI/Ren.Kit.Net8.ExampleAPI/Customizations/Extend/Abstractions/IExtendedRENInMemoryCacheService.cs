using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
public interface IExtendedRENInMemoryCacheService : IRENCacheService
{
    void AdditionalMethod();
}
