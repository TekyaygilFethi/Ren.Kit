# 🦌 REN.Kit.CacheKit

**REN.Kit.CacheKit** is an enterprise-grade, provider-agnostic caching toolkit for .NET.
It offers modern, high-performance caching with both **In-Memory** and **Redis** support through a clean and simple API.

> ✔️ Fully compatible with **.NET 8**, **.NET 9**, and **.NET 10**

---

## 🚀 Why REN.Kit.CacheKit?

| Feature | Status |
|--------|:-----:|
| In-Memory Cache | ✔️ |
| Redis Distributed Cache | ✔️ |
| Async + Sync APIs | ✔️ |
| Plug-and-play DI registration | ✔️ |
| Unified Cache Abstraction | ✔️ |
| Custom serialization | ✔️ |
| Provider-agnostic & Extensible | ✔️ |

- Abstracted caching interface → switch providers **without rewriting code**
- Developer controls **Redis multiplexer factory** & **lifetime**
- Safe defaults, strong performance, high flexibility

---

## ⚙️ Installation

```sh
dotnet add package REN.Kit.CacheKit
```

---

## 🧩 Choose Your Cache Provider

Cache provider is selected during DI registration.

### 🧠 In-Memory Cache (default & fastest)

```csharp
builder.Services.AddRENCaching(CacheType.InMemory);
```

---

### 🧱 Redis Cache (distributed & scalable)

```csharp
Func<IServiceProvider, IConnectionMultiplexer> redisMultiplexerProvider = sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var options = new ConfigurationOptions
    {
        EndPoints = { config["CacheConfiguration:RedisConfiguration:Url"] },
        DefaultDatabase = int.Parse(config["CacheConfiguration:RedisConfiguration:DatabaseId"] ?? "0"),
        User = config["CacheConfiguration:RedisConfiguration:Username"],
        Password = config["CacheConfiguration:RedisConfiguration:Password"],
        AbortOnConnectFail = bool.Parse(config["CacheConfiguration:RedisConfiguration:AbortOnConnectFail"] ?? "false")
    };
    return ConnectionMultiplexer.Connect(options);
};

builder.Services.AddRENCaching(
    CacheType.Redis,
    redisMultiplexerProvider,
    RedisMultiplexerLifetime.Singleton);
```

> 🧠 Recommended: `Singleton` multiplexer lifetime for production apps

---

## 🛠️ Usage

Inject the unified interface → provider becomes irrelevant:

```csharp
public class WeatherService(IRENCacheService cache)
{
    public async Task<Weather?> GetWeatherAsync(string city)
    {
        return await cache.GetOrCreateAsync(
            $"weather:{city}",
            async _ => await FetchWeatherFromApi(city),
            absoluteExpiration: TimeSpan.FromMinutes(15)
        );
    }
}
```

---

### Basic Operations

```csharp
cache.Set("user:1", user);
var user = cache.Get<User>("user:1");

await cache.RemoveAsync("user:1");
await cache.ClearAsync();
```

---

## 📝 Optional Configuration (appsettings.json)

```json
"CacheConfiguration": {
  "RedisConfiguration": {
    "Url": "localhost:6379",
    "DatabaseId": 0,
    "Username": "default",
    "Password": "mypwd",
    "AbortOnConnectFail": false
  }
}
```

Provides flexibility: Json, env variables, secrets, remote key stores—you decide.

---

## 🔌 Extensibility

Override anything easily:

```csharp
public class CustomRedisCache : RENRedisCacheService
{
    public CustomRedisCache(IConnectionMultiplexer mux)
        : base(mux) { }

    public override void Remove(string key)
        => Console.WriteLine($"Cache removed: {key}");
}
```

Register custom implementation:

```csharp
builder.Services.AddScoped<IRENCacheService, CustomRedisCache>();
```

---

## 📐 Why Redis Factory & Lifetime From Consumer?

✔ Architecture flexibility  
✔ Connection pooling correctness  
✔ Perfect for multi-tenant and high-scale systems  
✔ Easy test-ability with mocks  
✔ Zero configuration assumptions in the library

Your app decides how Redis should behave — **not the package**.

---

## 📚 Documentation

📘 Complete guides & best practices:  
➡ https://fethis-organization.gitbook.io/ren.kit-documentation/

---

## 🤝 Contributing

We welcome feature ideas and PRs!  
Let’s build a better caching experience together 🦌

---

**REN.Kit.CacheKit**  
Regular. Everyday. Normal.  
**Super-powered caching for .NET** 🚀
