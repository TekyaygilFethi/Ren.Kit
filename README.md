# REN.Kit

**REN.Kit** is a next-generation library suite that radically simplifies caching and data access for modern .NET applications—whether you’re building high-throughput APIs, distributed microservices, or robust enterprise solutions.

> _Regular. Everyday. Normal. Now, supercharged._

---

## 🚀 What is REN.Kit?

REN.Kit is an integrated toolbox for .NET developers. It delivers **plug-and-play solutions** for:
- **Caching** (In-Memory and Redis)
- **Database operations** (Unit of Work and Repository patterns)
- Seamless extensibility and customization for your unique requirements

Say goodbye to boilerplate code, complex configurations, or maintaining home-grown helpers. With REN.Kit, you can focus on delivering business value—**not reinventing the wheel**.

---

## 📦 Packages

REN.Kit is distributed as modular NuGet packages:

### [REN.Kit.DataKit](https://www.nuget.org/packages/REN.Kit.DataKit)
Enterprise-grade data access layer with:
- Unit of Work & Repository patterns, ready for production
- Seamless integration with **EF Core**, **MSSQL**, **PostgreSQL** (and any EF Core provider)
- LINQ, bulk, transaction support, and async operations
- Override and extend to fit your business needs

See: [REN.Kit.DataKit Documentation](https://fethis-organization.gitbook.io/ren.kit-documentation/ren.kit.datakit/introduction)

---

### [REN.Kit.CacheKit](https://www.nuget.org/packages/REN.Kit.CacheKit)
Ultra-fast, extensible caching with:
- First-class support for **In-Memory** and **Redis**
- Production-ready, configurable, and easy to use
- Supports async/sync, expiration policies, and custom strategies
- Plug-and-play: drop into any .NET 8 or .NET 9 project

See: [REN.Kit.CacheKit Documentation](https://fethis-organization.gitbook.io/ren.kit-documentation/ren.kit.cachekit/introduction)

---

## 🔥 Why Choose REN.Kit?

- **Modern:** Built from the ground up for .NET 8+ and cloud-native workloads
- **Plug-and-Play:** Integrate in minutes, not days—no boilerplate, no pain
- **Enterprise-Ready:** Trusted for high-scale APIs, distributed systems, and mission-critical apps
- **Extensible:** Easily override or extend any behavior—repositories, Unit of Work, or cache logic
- **Reliable:** Robust transactional support, ACID guarantees, and clean dependency injection
- **Well-documented:** Rich documentation and real-world samples to accelerate your onboarding

---

## ✨ Migrating from RenHelpers?

REN.Kit is the official, modern successor to [RenHelpers](https://www.nuget.org/packages/RENHelpers.DataAccessHelpers).  
It brings all your favorite features—now optimized, refactored, and future-proofed.  
Simply remove the old package, install REN.Kit.DataKit and/or REN.Kit.CacheKit, and follow the migration guide.

---

## 📚 Documentation

Full documentation (with guides, samples, and migration tips):  
[REN.Kit Documentation](https://fethis-organization.gitbook.io/ren.kit-documentation)

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](https://fethis-organization.gitbook.io/ren.kit-documentation/contribution) for guidelines and how to get started.

---

## 💬 Support & Community

- [GitHub Issues](https://github.com/TekyaygilFethi/Ren.Kit/issues) for bug reports and feature requests
- Pull Requests are encouraged—help us make REN.Kit even better!

---

## 🏁 Get Started Today

Install the packages, plug them in, and unlock clean, maintainable, and high-performance .NET code.

```shell
dotnet add package REN.Kit.DataKit
dotnet add package REN.Kit.CacheKit
```

**REN.Kit**  
_Regular. Everyday. Normal._  
**Just a little bit more awesome. Built for developers, by developers.**