namespace Ren.Kit.CacheKit.Abstractions;

/// <summary>
/// Unified, scalable, provider-agnostic cache service contract.
/// All methods are safe and work identically on InMemory and Redis implementations.
/// </summary>
public interface IRENCacheService
{
    /// <summary>
    /// Retrieves data of type <typeparamref name="T"/> from the cache.
    /// Returns null if not found or type mismatch.
    /// </summary>
    T? Get<T>(string cacheKey);

    /// <summary>
    /// Asynchronously retrieves data of type <typeparamref name="T"/> from the cache.
    /// Returns null if not found or type mismatch.
    /// </summary>
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a string value from the cache, or null if not found.
    /// </summary>
    string? Get(string cacheKey);

    /// <summary>
    /// Asynchronously retrieves a string value from the cache, or null if not found.
    /// </summary>
    Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores data in the cache with optional expiration settings.
    /// Sliding expiration is best-effort (Redis only supports absolute).
    /// </summary>
    void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

    /// <summary>
    /// Asynchronously stores data in the cache with optional expiration settings.
    /// Sliding expiration is best-effort (Redis only supports absolute).
    /// </summary>
    Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cache entry by key.
    /// </summary>
    void Remove(string cacheKey);

    /// <summary>
    /// Asynchronously removes a cache entry by key.
    /// </summary>
    Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completely clears all cache entries managed by this service.
    /// (Warning: In Redis, this may clear the whole database, use with caution!)
    /// </summary>
    void Clear();

    /// <summary>
    /// Asynchronously clears all cache entries managed by this service.
    /// (Warning: In Redis, this may clear the whole database, use with caution!)
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from the cache or, if not found, creates and stores it via the given factory.
    /// If factory returns null, nothing is cached and null is returned.
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default
    );
}

