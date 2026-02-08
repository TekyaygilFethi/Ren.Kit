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

    /// <summary>
    /// Adds or updates a hash field within the specified Redis key.
    /// If the key does not exist, a new Redis Hash is created.
    /// 
    /// TTL (if provided) applies at the HASH KEY level, not per-field.
    /// </summary>
    /// <typeparam name="T">Type of the object to cache.</typeparam>
    /// <param name="key">Redis hash key.</param>
    /// <param name="field">Field identifier within the hash.</param>
    /// <param name="value">Value to store.</param>
    /// <param name="absoluteExpiration">Optional expiration time for the key.</param>
    Task HashSetAsync<T>(
        string key,
        string field,
        T value,
        TimeSpan? absoluteExpiration = null
    );

    /// <summary>
    /// Gets a value stored in a hash field. Returns default if the field does not exist
    /// or value cannot be deserialized.
    /// </summary>
    /// <typeparam name="T">Expected type of the cached value.</typeparam>
    /// <param name="key">Redis hash key.</param>
    /// <param name="field">Field identifier within the hash.</param>
    Task<T?> HashGetAsync<T>(string key, string field);

    /// <summary>
    /// Removes a hash field from the Redis key.
    /// Returns true if the field existed and was removed; otherwise false.
    /// </summary>
    /// <param name="key">Redis hash key.</param>
    /// <param name="field">Field identifier to remove.</param>
    Task<bool> HashDeleteFieldAsync(string key, string field);

    /// <summary>
    /// Retrieves all field names stored within the Redis hash.
    /// Useful for listing keys like symbol identifiers.
    /// </summary>
    /// <param name="key">Redis hash key.</param>
    Task<HashSet<string>> HashGetAllFieldsAsync(string key);

    /// <summary>
    /// Retrieves all fields and values from a hash key,
    /// deserializing them into a Dictionary.
    /// </summary>
    /// <typeparam name="T">Expected type of all values in the hash.</typeparam>
    /// <param name="key">Redis hash key.</param>
    Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);

    /// <summary>
    /// Retrieves raw binary payload stored at <paramref name="cacheKey"/>.
    /// Returns null if not found.
    /// 
    /// Use this when the caller performs external serialization (e.g., MessagePack).
    /// </summary>
    byte[]? GetBytes(string cacheKey);

    /// <summary>
    /// Asynchronously retrieves raw binary payload stored at <paramref name="cacheKey"/>.
    /// Returns null if not found.
    /// 
    /// Use this when the caller performs external serialization (e.g., MessagePack).
    /// </summary>
    Task<byte[]?> GetBytesAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores raw binary payload at <paramref name="cacheKey"/> with optional absolute expiration.
    /// 
    /// Sliding expiration is not handled here; this method is format-agnostic and expects the caller
    /// to manage any higher-level policy if needed.
    /// </summary>
    void SetBytes(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Asynchronously stores raw binary payload at <paramref name="cacheKey"/> with optional absolute expiration.
    /// 
    /// Sliding expiration is not handled here; this method is format-agnostic and expects the caller
    /// to manage any higher-level policy if needed.
    /// </summary>
    Task SetBytesAsync(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates a hash field within the specified Redis key using raw binary payload.
    /// If the key does not exist, a new Redis Hash is created.
    /// 
    /// TTL (if provided) applies at the HASH KEY level, not per-field.
    /// </summary>
    Task HashSetBytesAsync(string key, string field, byte[] value, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Gets a raw binary payload stored in a hash field. Returns null if the field does not exist.
    /// </summary>
    Task<byte[]?> HashGetBytesAsync(string key, string field);

    /// <summary>
    /// Retrieves all fields and raw binary payloads from a hash key as a Dictionary.
    /// </summary>
    Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key);

    /// <summary>
    /// Retrieves multiple raw binary cache entries in a single Redis call.
    /// Missing keys are returned with null values.
    /// </summary>
    /// <param name="keys">Cache keys to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Dictionary<string, byte[]?>> GetBytesManyAsync(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default);
}

