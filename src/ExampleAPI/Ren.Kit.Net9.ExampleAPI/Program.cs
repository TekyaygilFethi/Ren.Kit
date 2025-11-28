using Microsoft.EntityFrameworkCore;
using Ren.Kit.CacheKit.Extensions;
using Ren.Kit.DataKit.Extensions;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Services;
using Ren.Kit.Net9.ExampleAPI.Customizations.ExtendAndOverride.Services;
using Ren.Kit.Net9.ExampleAPI.Customizations.Override.Services;
using Ren.Kit.Net9.ExampleAPI.Data.Database;
using StackExchange.Redis;
using static Ren.Kit.CacheKit.Extensions.RENExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        opts.JsonSerializerOptions.MaxDepth = 64;
    });

var configuration = builder.Configuration;

#region REN Cache Kit Register

Func<IServiceProvider, IConnectionMultiplexer> redisMultiplexerProvider = provider =>
{
    var redisOptions = new ConfigurationOptions
    {
        EndPoints = { configuration.GetSection("CacheConfiguration:RedisConfiguration:Url")?.Value },
        DefaultDatabase = int.Parse(configuration.GetSection("CacheConfiguration:RedisConfiguration:DatabaseId")?.Value ?? "0"),
        AbortOnConnectFail = bool.Parse(configuration.GetSection("CacheConfiguration:RedisConfiguration:AbortOnConnectFail")?.Value),
        User = configuration.GetSection("CacheConfiguration:RedisConfiguration:Username")?.Value,
        Password = configuration.GetSection("CacheConfiguration:RedisConfiguration:Password")?.Value
    };
    return ConnectionMultiplexer.Connect(redisOptions);
};

//For Standard In Memory Cache Service Injection
builder.Services.AddRENCaching(CacheType.InMemory); // comment this out if not used

////For Standard Redis Cache Service Injection
//builder.Services.AddRENCaching(CacheType.Redis, redisMultiplexerProvider, RedisMultiplexerLifetime.Singleton); // comment this out if not used

//// For Overrided In Memory Cache Service Injection
//builder.Services.AddRENCaching<OverridedRENInMemoryCacheService>(CacheType.InMemory); // comment this out if not used

//// For Overrided IRedis Cache Service Injection
//builder.Services.AddRENCaching<OverridedRENRedisCacheService>(CacheType.Redis, redisMultiplexerProvider, RedisMultiplexerLifetime.Singleton); // comment this out if not used

//// For Extended In Memory Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENInMemoryCacheService, ExtendedRENInMemoryCacheService>(CacheType.InMemory); // comment this out if not used

//// For Extended IRedis Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENRedisCacheService, ExtendedRENRedisCacheService>(CacheType.Redis, redisMultiplexerProvider, RedisMultiplexerLifetime.Singleton); // comment this out if not used

//// For Extended And Overrided In Memory Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENInMemoryCacheService, ExtendedAndOverridedRENInMemoryCacheService>(CacheType.InMemory); // comment this out if not used

//// For Extended And Overrided IRedis Cache Service Injection
//builder.Services.AddRENCaching<IExtendedRENRedisCacheService, ExtendedAndOverridedRENRedisCacheService>(CacheType.Redis, redisMultiplexerProvider, RedisMultiplexerLifetime.Singleton); // comment this out if not used
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
