using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
public interface IExtendedRENUnitOfWork<TDbContext> : IRENUnitOfWork<TDbContext> where TDbContext : DbContext
{
    void AdditionalMethod();
}

