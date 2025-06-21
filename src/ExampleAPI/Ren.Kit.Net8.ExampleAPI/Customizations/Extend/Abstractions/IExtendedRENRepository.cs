using Ren.Kit.DataKit.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions
{
    public interface IExtendedRENRepository<TEntity> : IRENRepository<TEntity> where TEntity : class
    {
        void AdditionalMethod();
    }
}
