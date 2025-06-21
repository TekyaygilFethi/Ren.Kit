using Microsoft.EntityFrameworkCore;
using Ren.Kit.Net9.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net9.ExampleAPI.Data.Database;

public class RenDbContext : DbContext
{
    public RenDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUserEntities(modelBuilder);
    }

    private void ConfigureUserEntities(ModelBuilder builder)
    {
        builder.Entity<Employee>()
            .HasOne(_ => _.Department)
            .WithMany(_ => _.Employees)
            .HasForeignKey(_ => _.DepartmentId);
    }
}