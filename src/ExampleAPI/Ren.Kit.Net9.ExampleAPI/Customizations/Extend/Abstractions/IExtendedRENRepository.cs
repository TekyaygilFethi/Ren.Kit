using Ren.Kit.DataKit.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions
{
    public interface IExtendedRENRepository<TEntity> : IRENRepository<TEntity> where TEntity : class
    {
        void AdditionalMethod();
    }
}
