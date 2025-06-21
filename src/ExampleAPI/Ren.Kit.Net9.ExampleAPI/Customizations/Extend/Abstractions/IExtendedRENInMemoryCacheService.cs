using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;
public interface IExtendedRENInMemoryCacheService : IRENCacheService
{
    void AdditionalMethod();
}
