# REN.Kit.DataKit

**REN.Kit.DataKit** is the next-generation evolution of the popular [RenHelpers.DataAccessHelpers](https://www.nuget.org/packages/RENHelpers.DataAccessHelpers) package—modernized, optimized, and fully reimagined for .NET 8, .NET 9, and beyond.

> ⚠️ **Notice:**  
> The legacy `RenHelpers` and `RenHelpers.DataAccessHelpers` packages are now obsolete. All new features and updates are exclusively delivered via REN.Kit.

---

## 🚀 What is REN.Kit.DataKit?

REN.Kit.DataKit is an **enterprise-ready library suite** designed to simplify and accelerate database operations in .NET projects.  
It provides robust, production-grade implementations of the **Unit of Work** and **Repository** patterns, along with advanced extensibility for custom business logic and transactions.

- **Battle-tested design patterns** (Unit of Work & Repository) out of the box  
> Fully compatible with **.NET 8**, **.NET 9**, and **.NET 10**
- Works seamlessly with **EF Core**, **MSSQL**, **PostgreSQL**, and more  
- Full LINQ, bulk, and transactional support  
- Highly extensible and override-friendly  
- Clean, maintainable, and production-proven architecture

---

## 🏁 Quick Start

**1. Install the Package**

```shell
dotnet add package REN.Kit.DataKit
```

**2. Register the Services**
In your `Program.cs`:

```csharp
builder.Services.RegisterRENDataServices();
```

**3. Use in Your Application**

Inject IRENUnitOfWork<TDbContext> into your controllers/services:
```csharp
[Route("api/[controller]")]
[ApiController]
public class UsersController(IRENUnitOfWork<AppDbContext> unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await unitOfWork.GetRepository<User>().GetListAsync();
        return Ok(users);
    }
}
```

## 🔥 Why REN.Kit.DataKit?

- **Modern:** Built for .NET 8+ and cloud-native workloads  
- **Extensible:** Easily override or extend any method, repository, or Unit of Work  
- **Production-Proven:** Trusted for both simple APIs and complex, enterprise-scale projects  
- **Plug-and-Play:** Integrate in minutes—no boilerplate required  
- **Reliable:** Robust transactional handling, ACID support, and rich configuration  

---

## 🏢 Enterprise-Grade Features

- Seamless integration with **MSSQL**, **PostgreSQL**, and all EF Core providers  
- LINQ, bulk, and transaction support  
- **Caching ready** (see [REN.Kit.CacheKit](https://www.nuget.org/packages/REN.Kit.CacheKit))  
- Full dependency injection support  
- Thorough documentation with real-world examples  

---

## 📦 Migrating from RenHelpers.DataAccessHelpers?

Migration is easy—your favorite features have been modernized and improved.  
**REN.Kit.DataKit** brings better performance, stronger extensibility, and support for the latest .NET/EF Core features.
You only need remove the RenHelpers.DataAccessHelpers package and install REN.Kit.DataKit. But please bear in mind that 
if you are using cache operations too, then you need to install [REN.Kit.CacheKit](https://www.nuget.org/packages/REN.Kit.CacheKit) as well.

---

## 💡 Looking for Caching?

Check out [REN.Kit.CacheKit](https://www.nuget.org/packages/REN.Kit.CacheKit) for enterprise-grade caching helpers.  
It integrates seamlessly with DataKit.

---

## 📚 Documentation

Explore the [Full Documentation](https://fethis-organization.gitbook.io/ren.kit-documentation/) for guides, examples, and migration tips.

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING](https://fethis-organization.gitbook.io/ren.kit-documentation/contribution) for guidelines.

---

**REN.Kit.DataKit**  
Regular. Everyday. Normal. Now, even more powerful.

_Built for developers, by developers.  
Get started with REN.Kit and modernize your .NET data access today!_
