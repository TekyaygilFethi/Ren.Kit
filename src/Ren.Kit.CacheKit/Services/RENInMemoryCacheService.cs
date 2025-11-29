using Microsoft.Extensions.Caching.Memory;
using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.CacheKit.Services;

/// <summary>
/// Thread-safe, high-performance in-memory cache service implementing IRENCacheService.
/// </summary>
public class RENInMemoryCacheService : IRENCacheService
{
    private readonly IMemoryCache _cache;
    private MemoryCacheEntryOptions _defaultOptions;

    public RENInMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
        _defaultOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
    }

    /// <inheritdoc/>
    public virtual T? Get<T>(string cacheKey)
    {
        if (_cache.TryGetValue(cacheKey, out var value) && value is T typed)
            return typed;
        return default;
    }

    /// <inheritdoc/>
    public virtual Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        => Task.FromResult(Get<T>(cacheKey));

    /// <inheritdoc/>
    public virtual string? Get(string cacheKey)
        => _cache.TryGetValue(cacheKey, out var value) ? value?.ToString() : null;

    /// <inheritdoc/>
    public virtual Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        => Task.FromResult(Get(cacheKey));

    /// <inheritdoc/>
    public virtual void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = slidingExpiration ?? _defaultOptions.SlidingExpiration
        };
        _cache.Set(cacheKey, data!, options);
    }

    /// <inheritdoc/>
    public virtual Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        Set(cacheKey, data, absoluteExpiration, slidingExpiration);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void Remove(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }

    /// <inheritdoc/>
    public virtual Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Remove(cacheKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
        // IMemoryCache does not support global clear, must be implemented if you track keys yourself.
        // This is left blank intentionally.
        throw new NotSupportedException("IMemoryCache does not natively support clearing all cache entries. Implement key tracking if needed.");
    }

    /// <inheritdoc/>
    public virtual Task ClearAsync(CancellationToken cancellationToken = default)
    {
        Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task<T?> GetOrCreateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is T value)
            return value;

        value = await factory(cancellationToken).ConfigureAwait(false);
        if (value is not null)
            Set(cacheKey, value, absoluteExpiration, slidingExpiration);
        return value;
    }

    /// <inheritdoc/>
    public Task HashSetAsync<T>(string key, string field, T value, TimeSpan? absoluteExpiration = null)
    {
        // This is left blank intentionally.
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<T?> HashGetAsync<T>(string key, string field)
    {
        // This is left blank intentionally.
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<bool> HashDeleteFieldAsync(string key, string field)
    {
        // This is left blank intentionally.
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<HashSet<string>> HashGetAllFieldsAsync(string key)
    {
        // This is left blank intentionally.
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
    {
        // This is left blank intentionally.
        throw new NotImplementedException();
    }
}
