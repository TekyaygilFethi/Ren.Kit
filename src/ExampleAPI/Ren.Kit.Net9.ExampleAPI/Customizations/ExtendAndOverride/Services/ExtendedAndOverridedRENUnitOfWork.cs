using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.DataKit.Services;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Override.Services;

public class ExtendedAndOverridedRENUnitOfWork<TDbContext>(TDbContext context) : RENUnitOfWork<TDbContext>(context), IExtendedRENUnitOfWork<TDbContext> where TDbContext : DbContext
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedAndOverridedRENUnitOfWork AdditionalMethod called.");
        // Implement your additional logic here
    }

    public override IRENRepository<TEntity>? GetRepository<TEntity>()
    {
        Console.WriteLine("ExtendedAndOverridedRENUnitOfWork GetRepository called");
        // You can add custom logic here before calling the base method
        return base.GetRepository<TEntity>();
    }
}
