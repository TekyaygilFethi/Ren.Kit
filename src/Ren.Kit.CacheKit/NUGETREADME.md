# REN.Kit.CacheKit

**REN.Kit.CacheKit** is the next-generation evolution of the popular [RenHelpers.DataAccessHelpers](https://www.nuget.org/packages/RENHelpers.DataAccessHelpers) package—modernized, optimized, and fully reimagined for .NET 8, .NET 9, and beyond.

> ⚠️ **Notice:**  
> The legacy `RenHelpers` and `RenHelpers.DataAccessHelpers` packages are now obsolete. All new features and updates are exclusively delivered via REN.Kit.
---

## 🚀 What is REN.Kit.CacheKit?

REN.Kit.CacheKit is an **enterprise-ready caching toolkit** for .NET applications.  
It provides robust, production-grade caching services for both **In-Memory** and **Redis** out-of-the-box, with advanced extensibility for custom logic.

- Lightning-fast in-memory caching
- Scalable, distributed Redis cache support
- Clean interface with synchronous and asynchronous APIs
- Full support for .NET 8 and .NET 9
- Effortless extensibility (override or extend any method)
- Plug-and-play integration—start caching in minutes

---

## 🏁 Quick Start

**1. Install the Package**

```shell
dotnet add package REN.Kit.CacheKit
```

**2. Configure Your Cache**
Add your cache configuration to `appsettings.json`:

```json
"CacheConfiguration": {
  "InMemoryConfiguration": {
    "TimeConfiguration": {
      "AbsoluteExpirationInHours": 12,
      "SlidingExpirationInMinutes": 30
    }
  },
  "RedisConfiguration": {
    "Url": "localhost:6379",
    "TimeConfiguration": {
      "AbsoluteExpirationInHours": 12
    },
    "DatabaseId": 0,
    "Username": "default",
    "Password": "mypwd",
    "AbortOnConnectFail": false,
    "IsAdmin": false
  }
}
```

**3. Register the Service in Program.cs**
```csharp
builder.Services.AddRENCaching(RegisterRENCaching.CacheType.InMemory); // For In-Memory caching

builder.Services.AddRENCaching(RegisterRENCaching.CacheType.Redis); // For Redis caching
```

**4. Inject and Use the Service**
```csharp
public class HomeController(IRENCacheService cacheService) : ControllerBase
{
    [HttpPost("set")]
    public async Task<IActionResult> SetCache([FromQuery] string key, [FromBody] object value)
    {
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(10));
        return Ok("Value cached.");
    }
}
```

## 🔥 Why REN.Kit.CacheKit?

- **Modern:** Designed for .NET 8+ and cloud-native workloads
- **Extensible:** Easily override or extend any caching behavior
- **Battle-Tested:** Ready for production and enterprise-scale projects
- **Plug-and-Play:** Integrate in minutes—no boilerplate required
- **Reliable:** Robust expiration policies, async/sync support, and easy migration

---

## 🏢 Enterprise-Grade Features

- Seamless support for **In-Memory** and **Redis** cache
- Automatic dependency injection
- Works with any .NET 8 or .NET 9 project
- Production-ready design and configuration
- Advanced extensibility for custom requirements

---

## 📦 Migrating from RenHelpers?

Migration is easy—your favorite caching features have been modernized and improved.  
Simply uninstall the old package and install **REN.Kit.CacheKit**.  
Check the documentation for full migration details and code samples.

---

## 📚 Documentation

Explore the [Full Documentation](https://fethis-organization.gitbook.io/ren.kit-documentation/) for guides, examples, and migration tips.

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING](https://fethis-organization.gitbook.io/ren.kit-documentation/contribution) for guidelines.

---


**REN.Kit.CacheKit**  
Regular. Everyday. Normal. Now, even faster and more reliable.

_Built for developers, by developers._  
**Start supercharging your .NET caching with REN.Kit today!**
