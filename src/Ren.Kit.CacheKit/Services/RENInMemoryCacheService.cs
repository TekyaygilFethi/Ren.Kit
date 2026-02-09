using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.CacheKit.Services
{
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

        public virtual T? Get<T>(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out var value) && value is T typed)
                return typed;
            return default;
        }

        public virtual Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
            => Task.FromResult(Get<T>(cacheKey));

        public virtual string? Get(string cacheKey)
            => _cache.TryGetValue(cacheKey, out var value) ? value?.ToString() : null;

        public virtual Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(cacheKey));

        public virtual void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = slidingExpiration ?? _defaultOptions.SlidingExpiration
            };
            _cache.Set(cacheKey, data!, options);
        }

        public virtual Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            Set(cacheKey, data, absoluteExpiration, slidingExpiration);
            return Task.CompletedTask;
        }

        public virtual void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        public virtual Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            Remove(cacheKey);
            return Task.CompletedTask;
        }

        public virtual void Clear()
            => throw new NotSupportedException("IMemoryCache does not natively support clearing all cache entries. Implement key tracking if needed.");

        public virtual Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Clear();
            return Task.CompletedTask;
        }

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

        public virtual Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return Task.FromResult(false);

            return Task.FromResult(_cache.TryGetValue(cacheKey, out _));
        }

        public virtual Task<bool> RefreshAsync(string cacheKey, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return Task.FromResult(false);

            if (!_cache.TryGetValue(cacheKey, out var value) || value is null)
                return Task.FromResult(false);

            if (value is byte[] bytes)
            {
                SetBytes(cacheKey, bytes, absoluteExpiration);
                return Task.FromResult(true);
            }

            _cache.Set(cacheKey, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _defaultOptions.SlidingExpiration
            });

            return Task.FromResult(true);
        }

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

        public virtual Task<long> IncrementAsync(string cacheKey, long by = 1, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return Task.FromResult(0L);

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
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = _defaultOptions.SlidingExpiration
                });

                return Task.FromResult(next);
            }
        }

        public Task HashSetAsync<T>(string key, string field, T value, TimeSpan? absoluteExpiration = null)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<T?> HashGetAsync<T>(string key, string field)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<bool> HashDeleteFieldAsync(string key, string field)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<HashSet<string>> HashGetAllFieldsAsync(string key)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<Dictionary<string, T?>> HashGetManyAsync<T>(string key, IEnumerable<string> fields, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task HashSetManyAsync<T>(string key, IReadOnlyCollection<KeyValuePair<string, T>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public virtual byte[]? GetBytes(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out var value) && value is byte[] bytes)
                return bytes;
            return null;
        }

        public virtual Task<byte[]?> GetBytesAsync(string cacheKey, CancellationToken cancellationToken = default)
            => Task.FromResult(GetBytes(cacheKey));

        public virtual void SetBytes(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow
            };

            _cache.Set(cacheKey, data, options);
        }

        public virtual Task SetBytesAsync(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            SetBytes(cacheKey, data, absoluteExpiration);
            return Task.CompletedTask;
        }

        public Task HashSetBytesAsync(string key, string field, byte[] value, TimeSpan? absoluteExpiration = null)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<byte[]?> HashGetBytesAsync(string key, string field)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

        public Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key)
            => throw new NotSupportedException("Hash operations are not supported by RENInMemoryCacheService.");

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
