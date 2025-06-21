using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.DataKit.Services;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Override.Services;

public class OverridedRENUnitOfWork<TDbContext>(TDbContext context) : RENUnitOfWork<TDbContext>(context), IRENUnitOfWork<TDbContext> where TDbContext : DbContext
{
    public override IRENRepository<TEntity>? GetRepository<TEntity>()
    {
        Console.WriteLine("OverridedRENUnitOfWork GetRENRepository called");
        // You can add custom logic here before calling the base method
        return base.GetRepository<TEntity>();
    }
}
