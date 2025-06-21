using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Services;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Override.Services;

public class ExtendedRENUnitOfWork<TDbContext>(TDbContext context) : RENUnitOfWork<TDbContext>(context), IExtendedRENUnitOfWork<TDbContext> where TDbContext : DbContext
{
    public void AdditionalMethod()
    {
        Console.WriteLine("ExtendedRENUnitOfWork AdditionalMethod called.");
        // Implement your additional logic here
    }
}
