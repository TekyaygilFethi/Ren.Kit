using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ren.Kit.Net8.ExampleAPI.Data.Database;

public class RenDbContextFactory : IDesignTimeDbContextFactory<RenDbContext>
{
    public RenDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<RenDbContext>();
        var connectionString = configuration.GetConnectionString("MSSQL");

        builder.UseSqlServer(connectionString);

        return new RenDbContext(builder.Options);
    }
}
