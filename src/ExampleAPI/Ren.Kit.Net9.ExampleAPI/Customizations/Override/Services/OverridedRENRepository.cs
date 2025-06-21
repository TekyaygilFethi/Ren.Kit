using Ren.Kit.DataKit.Services;
using Ren.Kit.Net9.ExampleAPI.Data.Database;
using System.Linq.Expressions;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Override.Services;

public class OverridedRENRepository<TEntity>(RenDbContext context) : RENRepository<TEntity>(context) where TEntity : class
{
    public override TEntity? GetSingle(Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool isReadOnly = false)
    {
        Console.WriteLine("OverridedRENRepository GetSingle called");
        // You can add custom logic here before calling the base method
        return base.GetSingle(filter, include, isReadOnly);
    }
}
