# 🦌 REN.Kit.CacheKit

**REN.Kit.CacheKit** is an enterprise-grade, provider-agnostic caching toolkit for .NET — now fully updated with **.NET 10 support** and an even **simpler DI registration** experience.

Designed to give you performance, flexibility, and clean architecture — without the configuration headache 🚀

---

## 🌟 What’s New?

| Feature | Status |
|--------|:-----:|
| .NET 10 Support | ✔️ |
| Reduced DI complexity | ✔️ |
| Unified Cache Abstraction | ✔️ |
| Redis Multiplexer Lifetime Control | ✔️ |
| Zero assumptions on your architecture | ✔️ |

Just choose the provider you want — everything else is handled for you.

---

## ⚡ Installation

```sh
dotnet add package REN.Kit.CacheKit
```

---

## 🧩 Easy Provider Selection

Cache provider is selected directly in **service registration**:

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

> 🧠 Best practice: Use `Singleton` multiplexer lifetime in production

---

## 🛠️ Usage

Inject the unified interface — your code doesn’t care which provider is active:

```csharp
public class WeatherService(IRENCacheService cache)
{
    public async Task<Weather?> GetWeatherAsync(string city)
    {
        return await cache.GetOrCreateAsync(
            $"weather:{city}",
            async _ => await FetchWeatherFromApi(city),
            TimeSpan.FromMinutes(15)
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

## 📝 Optional Redis Configuration

```json
"CacheConfiguration": {
  "UseDefaultAbsoluteExpirationWhenNull": "true",
  "RedisConfiguration": {
    "Url": "localhost:6379",
    "DatabaseId": 0,
    "Username": "default",
    "Password": "mypwd",
    "AbortOnConnectFail": false
  }
}
```

Use JSON, Env vars, secret managers — **REN.Kit does not force any configuration style on you**.

---

## 🔌 Extensibility

Override anything, add your own logic:

```csharp
public class CustomRedisCache : RENRedisCacheService
{
    public CustomRedisCache(IConnectionMultiplexer mux)
        : base(mux) { }

    public override void Remove(string key)
        => Console.WriteLine($"Removed: {key}");
}
```

Register your custom implementation:

```csharp
builder.Services.AddScoped<IRENCacheService, CustomRedisCache>();
```

---

## 🔍 Why the Redis Factory & Lifetime Are External?

✔ Architecture freedom  
✔ Correct resource management (no hidden multiplexers!)  
✔ Works with multi-tenant & advanced redis topologies  
✔ Fully testable (mocks allowed)  
✔ Provider behavior controlled by **you**, not by the library  

> Your app decides how Redis should work — not CacheKit 🦌

---

## 📚 Documentation

📘 Full docs & best practices:  
➡ https://fethis-organization.gitbook.io/ren.kit-documentation/

---

## 🤝 Contributing

We love contributions! 🎯  
Whether it’s a feature idea, PR, or bug report — let’s build a better caching toolkit together.

---

**REN.Kit.CacheKit**  
Regular. Everyday. Normal.  
Now even faster — **Power up your .NET caching today** 🚀
