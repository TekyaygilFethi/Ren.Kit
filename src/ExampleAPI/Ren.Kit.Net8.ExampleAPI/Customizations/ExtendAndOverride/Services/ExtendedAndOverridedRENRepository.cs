using Ren.Kit.DataKit.Services;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net8.ExampleAPI.Data.Database;
using System.Linq.Expressions;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.ExtendAndOverride.Services;

public class ExtendedAndOverridedRENRepository<TEntity>(RenDbContext context) : RENRepository<TEntity>(context), IExtendedRENRepository<TEntity> where TEntity : class
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedAndOverridedRENRepository AdditionalMethod called.");
        // Implement your additional logic here
    }

    public override List<TEntity>? GetList(Expression<Func<TEntity, bool>>? filter = null,
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
       Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
       bool isReadOnly = false)
    {
        Console.WriteLine("ExtendedAndOverridedRENRepository GetList called");

        var entities = base.GetList(filter, orderBy, include, isReadOnly);

        return entities;
    }
}