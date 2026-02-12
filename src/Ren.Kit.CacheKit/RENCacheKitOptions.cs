namespace Ren.Kit.CacheKit;

public class RENCacheKitOptions
{
    public CacheConfigurationOptions CacheConfiguration { get; set; } = new();
}

public class CacheConfigurationOptions
{
    public bool UseDefaultAbsoluteExpirationWhenNull { get; set; } = true;
    public RedisConfigurationOptions RedisConfiguration { get; set; } = new();
    public InMemoryConfigurationOptions InMemoryConfiguration { get; set; } = new();
}

public class RedisConfigurationOptions
{
    public string? Url { get; set; }
    public int DatabaseId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool AbortOnConnectFail { get; set; }
    public bool IsAdmin { get; set; }

    public TimeConfigurationOptions TimeConfiguration { get; set; } = new();
}

public class InMemoryConfigurationOptions
{
    public TimeConfigurationOptions TimeConfiguration { get; set; } = new();
}

public class TimeConfigurationOptions
{
    public int AbsoluteExpirationInHours { get; set; } = 12;
    public int SlidingExpirationInMinutes { get; set; } = 30;
}

