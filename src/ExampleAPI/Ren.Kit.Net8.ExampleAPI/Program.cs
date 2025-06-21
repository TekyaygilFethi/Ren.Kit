using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Extensions;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Services;
using Ren.Kit.Net8.ExampleAPI.Customizations.ExtendAndOverride.Services;
using Ren.Kit.Net8.ExampleAPI.Customizations.Override.Services;
using Ren.Kit.Net8.ExampleAPI.Data.Database;
using Ren.Kit.Net8.ExampleAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        opts.JsonSerializerOptions.MaxDepth = 64;
    });

#region REN Cache Kit Register

//For Standard In Memory Cache Service Injection
builder.Services.AddRENCaching(RegisterRENCaching.CacheType.InMemory); // comment this out if not used

////For Standard Redis Cache Service Injection
//builder.Services.AddRENCaching(RegisterRENCaching.CacheType.Redis); // comment this out if not used

//// For Overrided In Memory Cache Service Injection
//builder.Services.AddRENCaching<OverridedRENInMemoryCacheService>(RegisterRENCaching.CacheType.InMemory); // comment this out if not used

//// For Overrided IRedis Cache Service Injection
//builder.Services.AddRENCaching<OverridedRENRedisCacheService>(RegisterRENCaching.CacheType.Redis); // comment this out if not used

//// For Extended In Memory Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENInMemoryCacheService, ExtendedRENInMemoryCacheService>(RegisterRENCaching.CacheType.InMemory); // comment this out if not used

//// For Extended IRedis Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENRedisCacheService, ExtendedRENRedisCacheService>(RegisterRENCaching.CacheType.Redis); // comment this out if not used

//// For Extended And Overrided In Memory Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENInMemoryCacheService, ExtendedAndOverridedRENInMemoryCacheService>(RegisterRENCaching.CacheType.InMemory); // comment this out if not used

//// For Extended And Overrided IRedis Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENRedisCacheService, ExtendedAndOverridedRENRedisCacheService>(RegisterRENCaching.CacheType.Redis); // comment this out if not used
#endregion

// REN Database Injections
builder.Services.AddDbContext<RenDbContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")); });

#region REN Data Kit Register

// For Standard Unit Of Work Injection
builder.Services.RegisterRENDataServices(); // comment this out if not used

//// For Overrided Unit Of Work Injection
//builder.Services.RegisterRENDataServices(unitOfWorkOpenType: typeof(OverridedRENUnitOfWork<>)); // comment this out if not used

//// For Extended Unit Of Work Injection
//builder.Services.RegisterRENDataServices<IExtendedRENUnitOfWork<RenDbContext>, ExtendedRENUnitOfWork<RenDbContext>>(); // comment this out if not used

//// For Extended And Overrided Unit Of Work Injection
//builder.Services.RegisterRENDataServices<IExtendedRENUnitOfWork<RenDbContext>, ExtendedAndOverridedRENUnitOfWork<RenDbContext>>(); // comment this out if not used

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
