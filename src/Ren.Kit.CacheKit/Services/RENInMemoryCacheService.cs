using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.CacheKit.Services
{
    public class RENInMemoryCacheService : IRENCacheService
    {
        private readonly IMemoryCache _cache;
        private MemoryCacheEntryOptions _defaultOptions;

        private int _defaultAbsoluteExpirationHours = 12;
        private int _defaultSlidingExpirationMinutes = 30;
        private bool _useDefaultAbsoluteExpirationWhenNull = true;

        public RENInMemoryCacheService(IMemoryCache cache)
            : this(cache, null)
        {
        }

        public RENInMemoryCacheService(IMemoryCache cache, IOptions<RENCacheKitOptions>? options = null)
        {
            _cache = cache;

            var opt = options?.Value?.CacheConfiguration;

            if (opt is not null)
            {
                _useDefaultAbsoluteExpirationWhenNull =
                    opt.UseDefaultAbsoluteExpirationWhenNull;

                var timeConfig = opt.InMemoryConfiguration?.TimeConfiguration;
                if (timeConfig is not null)
                {
                    _defaultAbsoluteExpirationHours =
                        timeConfig.AbsoluteExpirationInHours;

                    _defaultSlidingExpirationMinutes =
                        timeConfig.SlidingExpirationInMinutes;
                }
            }

            _defaultOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromHours(_defaultAbsoluteExpirationHours),

                SlidingExpiration =
                    TimeSpan.FromMinutes(_defaultSlidingExpirationMinutes)
            };
        }

        /// <inheritdoc />
        public virtual T? Get<T>(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out var value) && value is T typed)
                return typed;
            return default;
        }

        /// <inheritdoc />
        public virtual Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
            => Task.FromResult(Get<T>(cacheKey));

        /// <inheritdoc />
        public virtual string? Get(string cacheKey)
            => _cache.TryGetValue(cacheKey, out var value) ? value?.ToString() : null;

        /// <inheritdoc />
        public virtual Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(cacheKey));

        /// <inheritdoc />
        public virtual void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            TimeSpan? abs =
                absoluteExpiration
                ?? (_useDefaultAbsoluteExpirationWhenNull ? _defaultOptions.AbsoluteExpirationRelativeToNow : null);

            TimeSpan? sliding = slidingExpiration;

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = abs,
                SlidingExpiration = sliding
            };

            _cache.Set(cacheKey, data!, options);
        }

        /// <inheritdoc />
        public virtual Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            Set(cacheKey, data, absoluteExpiration, slidingExpiration);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        /// <inheritdoc />
        public virtual Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            Remove(cacheKey);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual void Clear()
            => throw new NotSupportedException("IMemoryCache does not natively support clearing all cache entries. Implement key tracking if needed.");

        /// <inheritdoc />
        public virtual Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Clear();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return Task.FromResult(false);

            return Task.FromResult(_cache.TryGetValue(cacheKey, out _));
        }

        /// <inheritdoc />
        public virtual Task<bool> RefreshAsync(string cacheKey, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return Task.FromResult(false);

            if (!_cache.TryGetValue(cacheKey, out var value) || value is null)
                return Task.FromResult(false);

            TimeSpan? abs =
                absoluteExpiration
                ?? (_useDefaultAbsoluteExpirationWhenNull ? _defaultOptions.AbsoluteExpirationRelativeToNow : null);

            if (value is byte[] bytes)
            {
                SetBytes(cacheKey, bytes, abs);
                return Task.FromResult(true);
            }

            _cache.Set(cacheKey, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = abs,
                SlidingExpiration = _defaultOptions.SlidingExpiration
            });

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public virtual Task<long> RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (keys is null)
                return Task.FromResult(0L);

            long removed = 0;

            foreach (var k in keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(k)) continue;

                if (_cache.TryGetValue(k, out _))
                {
                    _cache.Remove(k);
                    removed++;
                }
            }

            return Task.FromResult(removed);
        }

        /// <inheritdoc />
        public virtual Task<long> IncrementAsync(string cacheKey, long by = 1, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return Task.FromResult(0L);

            TimeSpan? abs =
                absoluteExpiration
                ?? (_useDefaultAbsoluteExpirationWhenNull ? _defaultOptions.AbsoluteExpirationRelativeToNow : null);

            lock (_cache)
            {
                long current = 0;

                if (_cache.TryGetValue(cacheKey, out var obj))
                {
                    if (obj is long l) current = l;
                    else if (obj is int i) current = i;
                }

                var next = checked(current + by);

                _cache.Set(cacheKey, next, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = abs,
                    SlidingExpiration = _defaultOptions.SlidingExpiration
                });

                return Task.FromResult(next);
            }
        }

        /// <inheritdoc />
        public Task HashSetAsync<T>(string key, string field, T value, TimeSpan? absoluteExpiration = null)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<T?> HashGetAsync<T>(string key, string field)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<bool> HashDeleteFieldAsync(string key, string field)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<HashSet<string>> HashGetAllFieldsAsync(string key)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<Dictionary<string, T?>> HashGetManyAsync<T>(string key, IEnumerable<string> fields, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task HashSetManyAsync<T>(string key, IReadOnlyCollection<KeyValuePair<string, T>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public virtual byte[]? GetBytes(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out var value) && value is byte[] bytes)
                return bytes;
            return null;
        }

        /// <inheritdoc />
        public virtual Task<byte[]?> GetBytesAsync(string cacheKey, CancellationToken cancellationToken = default)
            => Task.FromResult(GetBytes(cacheKey));

        /// <inheritdoc />
        public virtual void SetBytes(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null)
        {
            TimeSpan? abs =
                absoluteExpiration
                ?? (_useDefaultAbsoluteExpirationWhenNull ? _defaultOptions.AbsoluteExpirationRelativeToNow : null);

            _cache.Set(cacheKey, data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = abs
            });
        }

        /// <inheritdoc />
        public virtual Task SetBytesAsync(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            SetBytes(cacheKey, data, absoluteExpiration);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task HashSetBytesAsync(string key, string field, byte[] value, TimeSpan? absoluteExpiration = null)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<byte[]?> HashGetBytesAsync(string key, string field)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        /// <inheritdoc />
        public virtual Task<Dictionary<string, byte[]?>> GetBytesManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (keys is null)
                return Task.FromResult(new Dictionary<string, byte[]?>(0, StringComparer.Ordinal));

            var arr = keys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (arr.Length == 0)
                return Task.FromResult(new Dictionary<string, byte[]?>(0, StringComparer.Ordinal));

            var dict = new Dictionary<string, byte[]?>(arr.Length, StringComparer.Ordinal);

            foreach (var key in arr)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_cache.TryGetValue(key, out var value) && value is byte[] bytes)
                    dict[key] = bytes;
                else
                    dict[key] = null;
            }

            return Task.FromResult(dict);
        }

        /// <inheritdoc />
        public virtual Task SetBytesManyAsync(IReadOnlyCollection<KeyValuePair<string, byte[]>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (items is null || items.Count == 0)
                return Task.CompletedTask;

            foreach (var kv in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(kv.Key))
                    continue;

                if (kv.Value is null || kv.Value.Length == 0)
                    continue;

                SetBytes(kv.Key, kv.Value, absoluteExpiration);
            }

            return Task.CompletedTask;
        }
    }
}
