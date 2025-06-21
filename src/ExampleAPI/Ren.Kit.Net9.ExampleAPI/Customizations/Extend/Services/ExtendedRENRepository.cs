using Ren.Kit.DataKit.Services;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Data.Database;

namespace Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Services;

public class ExtendedRENRepository<TEntity>(RenDbContext dbContext) : RENRepository<TEntity>(dbContext), IExtendedRENRepository<TEntity> where TEntity : class
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedRENRepository AdditionalMethod called.");
        // Implement your additional logic here
    }
}