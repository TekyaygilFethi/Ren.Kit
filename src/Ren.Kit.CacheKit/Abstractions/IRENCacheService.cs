namespace Ren.Kit.CacheKit.Abstractions
{
    public interface IRENCacheService
    {
        /// <summary>
        /// Retrieves data of type <typeparamref name="T"/> from the cache.
        /// Returns default if not found or type mismatch.
        /// </summary>
        /// <typeparam name="T">Expected type.</typeparam>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>Cached value or default.</returns>
        T? Get<T>(string cacheKey);

        /// <summary>
        /// Asynchronously retrieves data of type <typeparamref name="T"/> from the cache.
        /// Returns default if not found or type mismatch.
        /// </summary>
        /// <typeparam name="T">Expected type.</typeparam>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Cached value or default.</returns>
        Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a string value from the cache.
        /// Returns null if not found.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>Cached string or null.</returns>
        string? Get(string cacheKey);

        /// <summary>
        /// Asynchronously retrieves a string value from the cache.
        /// Returns null if not found.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Cached string or null.</returns>
        Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores data in the cache with optional expiration settings.
        /// Sliding expiration is best-effort (Redis only supports absolute).
        /// </summary>
        /// <typeparam name="T">Type of the value to store.</typeparam>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="data">Value to store.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration.</param>
        /// <param name="slidingExpiration">Optional sliding expiration.</param>
        void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

        /// <summary>
        /// Asynchronously stores data in the cache with optional expiration settings.
        /// Sliding expiration is best-effort (Redis only supports absolute).
        /// </summary>
        /// <typeparam name="T">Type of the value to store.</typeparam>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="data">Value to store.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration.</param>
        /// <param name="slidingExpiration">Optional sliding expiration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a cache entry by key.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        void Remove(string cacheKey);

        /// <summary>
        /// Asynchronously removes a cache entry by key.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all cache entries managed by this service.
        /// </summary>
        void Clear();

        /// <summary>
        /// Asynchronously clears all cache entries managed by this service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a value from the cache or, if not found, creates and stores it via the given factory.
        /// If factory returns null, nothing is cached and null is returned.
        /// </summary>
        /// <typeparam name="T">Expected type.</typeparam>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="factory">Factory to create value on cache miss.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration.</param>
        /// <param name="slidingExpiration">Optional sliding expiration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Cached or newly created value, or null.</returns>
        Task<T?> GetOrCreateAsync<T>(
            string cacheKey,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a key exists in the underlying cache.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if exists; otherwise false.</returns>
        Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes / touches TTL for a key (best-effort).
        /// Returns false if key does not exist.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="absoluteExpiration">New TTL to apply. If null, provider default TTL is used (best-effort).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if refreshed; otherwise false.</returns>
        Task<bool> RefreshAsync(string cacheKey, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes multiple keys.
        /// Returns number of removed keys (best-effort for InMemory).
        /// </summary>
        /// <param name="keys">Keys to remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Count of removed keys.</returns>
        Task<long> RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically increments a numeric value stored at key.
        /// If the key does not exist, it is created with initial value = 0 before increment.
        /// TTL (if provided) is applied to the key (best-effort).
        /// </summary>
        /// <param name="cacheKey">Counter key.</param>
        /// <param name="by">Increment amount.</param>
        /// <param name="absoluteExpiration">Optional TTL.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The incremented value.</returns>
        Task<long> IncrementAsync(string cacheKey, long by = 1, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds or updates a hash field within the specified key.
        /// TTL applies at the hash key level.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Hash key.</param>
        /// <param name="field">Field name.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="absoluteExpiration">Optional expiration for the hash key.</param>
        Task HashSetAsync<T>(string key, string field, T value, TimeSpan? absoluteExpiration = null);

        /// <summary>
        /// Gets a value stored in a hash field.
        /// Returns default if field missing or cannot be deserialized.
        /// </summary>
        /// <typeparam name="T">Expected type.</typeparam>
        /// <param name="key">Hash key.</param>
        /// <param name="field">Field name.</param>
        /// <returns>Field value or default.</returns>
        Task<T?> HashGetAsync<T>(string key, string field);

        /// <summary>
        /// Removes a hash field.
        /// </summary>
        /// <param name="key">Hash key.</param>
        /// <param name="field">Field name.</param>
        /// <returns>True if removed; otherwise false.</returns>
        Task<bool> HashDeleteFieldAsync(string key, string field);

        /// <summary>
        /// Retrieves all field names stored in a hash.
        /// </summary>
        /// <param name="key">Hash key.</param>
        /// <returns>Set of field names.</returns>
        Task<HashSet<string>> HashGetAllFieldsAsync(string key);

        /// <summary>
        /// Retrieves all fields and values from a hash.
        /// </summary>
        /// <typeparam name="T">Expected value type.</typeparam>
        /// <param name="key">Hash key.</param>
        /// <returns>Dictionary of field/value.</returns>
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);

        /// <summary>
        /// Gets many fields from a hash in a single operation.
        /// Missing fields are returned with null values.
        /// </summary>
        /// <typeparam name="T">Expected value type.</typeparam>
        /// <param name="key">Hash key.</param>
        /// <param name="fields">Fields to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dictionary of field/value (null if missing or invalid).</returns>
        Task<Dictionary<string, T?>> HashGetManyAsync<T>(string key, IEnumerable<string> fields, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets many fields on a hash in a single operation.
        /// TTL applies at the hash key level.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Hash key.</param>
        /// <param name="items">Field/value pairs.</param>
        /// <param name="absoluteExpiration">Optional expiration for the hash key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task HashSetManyAsync<T>(string key, IReadOnlyCollection<KeyValuePair<string, T>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves raw binary payload stored at the given key.
        /// Returns null if not found.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>Binary payload or null.</returns>
        byte[]? GetBytes(string cacheKey);

        /// <summary>
        /// Asynchronously retrieves raw binary payload stored at the given key.
        /// Returns null if not found.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Binary payload or null.</returns>
        Task<byte[]?> GetBytesAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores raw binary payload.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="data">Binary payload.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration.</param>
        void SetBytes(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null);

        /// <summary>
        /// Asynchronously stores raw binary payload.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="data">Binary payload.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetBytesAsync(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds or updates a hash field within the specified key using raw binary payload.
        /// TTL applies at the hash key level.
        /// </summary>
        /// <param name="key">Hash key.</param>
        /// <param name="field">Field name.</param>
        /// <param name="value">Binary payload.</param>
        /// <param name="absoluteExpiration">Optional expiration for the hash key.</param>
        Task HashSetBytesAsync(string key, string field, byte[] value, TimeSpan? absoluteExpiration = null);

        /// <summary>
        /// Gets a raw binary payload stored in a hash field.
        /// Returns null if the field does not exist.
        /// </summary>
        /// <param name="key">Hash key.</param>
        /// <param name="field">Field name.</param>
        /// <returns>Binary payload or null.</returns>
        Task<byte[]?> HashGetBytesAsync(string key, string field);

        /// <summary>
        /// Retrieves all fields and raw binary payloads from a hash key.
        /// </summary>
        /// <param name="key">Hash key.</param>
        /// <returns>Dictionary of field/binary payload.</returns>
        Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key);

        /// <summary>
        /// Retrieves multiple raw binary cache entries in a single operation.
        /// Missing keys are returned with null values.
        /// </summary>
        /// <param name="keys">Cache keys to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dictionary of key/binary payload (null if missing).</returns>
        Task<Dictionary<string, byte[]?>> GetBytesManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores multiple raw binary payloads.
        /// TTL (if provided) is applied per key.
        /// </summary>
        /// <param name="items">Key/value pairs to store.</param>
        /// <param name="absoluteExpiration">Optional TTL applied to each key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetBytesManyAsync(IReadOnlyCollection<KeyValuePair<string, byte[]>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);
    }
}
