using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ren.Kit.CacheKit.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace Ren.Kit.CacheKit.Services
{
    public class RENRedisCacheService : IRENCacheService
    {
        private readonly IDatabase _cacheDb;
        private readonly IConnectionMultiplexer _connection;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private int _defaultAbsoluteExpirationHours = 12;

        private const bool UseEnvelopeOnlyWhenSlidingProvided = true;

        internal sealed record CacheEnvelope<T>(
            T Value,
            DateTimeOffset CreatedAt,
            DateTimeOffset? AbsoluteExpiresAt,
            TimeSpan? SlidingExpiration
        );

        public RENRedisCacheService(IConnectionMultiplexer connection)
        {
            _connection = connection;
            _cacheDb = _connection.GetDatabase();
        }

        public virtual T? Get<T>(string cacheKey)
        {
            var data = _cacheDb.StringGet(cacheKey);
            if (!data.HasValue) return default;

            var json = (string?)data;
            if (string.IsNullOrWhiteSpace(json)) return default;

            if (TryDeserializeEnvelope(json, out CacheEnvelope<T>? env) && env is not null)
            {
                var now = DateTimeOffset.UtcNow;

                if (env.AbsoluteExpiresAt is not null && env.AbsoluteExpiresAt <= now)
                {
                    _ = _cacheDb.KeyDelete(cacheKey);
                    return default;
                }

                TouchSliding(cacheKey, env, now);
                return env.Value;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public virtual async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = await _cacheDb.StringGetAsync(cacheKey).ConfigureAwait(false);
            if (!data.HasValue) return default;

            var json = (string?)data;
            if (string.IsNullOrWhiteSpace(json)) return default;

            if (TryDeserializeEnvelope(json, out CacheEnvelope<T>? env) && env is not null)
            {
                var now = DateTimeOffset.UtcNow;

                if (env.AbsoluteExpiresAt is not null && env.AbsoluteExpiresAt <= now)
                {
                    _ = _cacheDb.KeyDeleteAsync(cacheKey);
                    return default;
                }

                await TouchSlidingAsync(cacheKey, env, now).ConfigureAwait(false);
                return env.Value;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public virtual string? Get(string cacheKey)
        {
            var value = _cacheDb.StringGet(cacheKey);
            return value.HasValue ? value.ToString() : null;
        }

        public virtual async Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await _cacheDb.StringGetAsync(cacheKey).ConfigureAwait(false);
            return value.HasValue ? value.ToString() : null;
        }

        public virtual void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var now = DateTimeOffset.UtcNow;
            var effectiveAbsolute = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);

            if (UseEnvelopeOnlyWhenSlidingProvided && slidingExpiration is null)
            {
                var legacyJson = JsonSerializer.Serialize(data, _jsonOptions);
                _cacheDb.StringSet(cacheKey, legacyJson, effectiveAbsolute);
                return;
            }

            var absAt = now.Add(effectiveAbsolute);

            var env = new CacheEnvelope<T>(
                data,
                now,
                absAt,
                slidingExpiration
            );

            var json = JsonSerializer.Serialize(env, _jsonOptions);
            var ttl = ComputeTtl(now, absAt, slidingExpiration);

            _cacheDb.StringSet(cacheKey, json, ttl);
        }

        public virtual async Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var now = DateTimeOffset.UtcNow;
            var effectiveAbsolute = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);

            if (UseEnvelopeOnlyWhenSlidingProvided && slidingExpiration is null)
            {
                var legacyJson = JsonSerializer.Serialize(data, _jsonOptions);
                await _cacheDb.StringSetAsync(cacheKey, legacyJson, effectiveAbsolute).ConfigureAwait(false);
                return;
            }

            var absAt = now.Add(effectiveAbsolute);

            var env = new CacheEnvelope<T>(
                data,
                now,
                absAt,
                slidingExpiration
            );

            var json = JsonSerializer.Serialize(env, _jsonOptions);
            var ttl = ComputeTtl(now, absAt, slidingExpiration);

            await _cacheDb.StringSetAsync(cacheKey, json, ttl).ConfigureAwait(false);
        }

        public virtual void Remove(string cacheKey)
            => _cacheDb.KeyDelete(cacheKey);

        public virtual async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _cacheDb.KeyDeleteAsync(cacheKey).ConfigureAwait(false);
        }

        public virtual void Clear()
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            server.FlushDatabase(_cacheDb.Database);
        }

        public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            await server.FlushDatabaseAsync(_cacheDb.Database).ConfigureAwait(false);
        }

        public virtual async Task<T?> GetOrCreateAsync<T>(
            string cacheKey,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            var cached = await GetAsync<T>(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cached is not null)
                return cached;

            var value = await factory(cancellationToken).ConfigureAwait(false);
            if (value is not null)
                await SetAsync(cacheKey, value, absoluteExpiration, slidingExpiration, cancellationToken).ConfigureAwait(false);

            return value;
        }

        public virtual async Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return false;

            try
            {
                return await _cacheDb.KeyExistsAsync(cacheKey).ConfigureAwait(false);
            }
            catch (RedisTimeoutException)
            {
                return false;
            }
            catch (RedisConnectionException)
            {
                return false;
            }
        }

        public virtual async Task<bool> RefreshAsync(string cacheKey, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return false;

            var ttl = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
            if (ttl <= TimeSpan.Zero)
                ttl = TimeSpan.FromSeconds(1);

            try
            {
                return await _cacheDb.KeyExpireAsync(cacheKey, ttl).ConfigureAwait(false);
            }
            catch (RedisTimeoutException)
            {
                return false;
            }
            catch (RedisConnectionException)
            {
                return false;
            }
        }

        public virtual async Task<long> RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (keys is null) return 0;

            var arr = keys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.Ordinal)
                .Select(k => (RedisKey)k)
                .ToArray();

            if (arr.Length == 0) return 0;

            try
            {
                return await _cacheDb.KeyDeleteAsync(arr).ConfigureAwait(false);
            }
            catch (RedisTimeoutException)
            {
                return 0;
            }
            catch (RedisConnectionException)
            {
                return 0;
            }
        }

        public virtual async Task<long> IncrementAsync(string cacheKey, long by = 1, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return 0;

            var ttl = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
            if (ttl <= TimeSpan.Zero)
                ttl = TimeSpan.FromSeconds(1);

            try
            {
                var next = await _cacheDb.StringIncrementAsync(cacheKey, by).ConfigureAwait(false);

                if (absoluteExpiration.HasValue)
                    _ = _cacheDb.KeyExpireAsync(cacheKey, ttl);

                return (long)next;
            }
            catch (RedisTimeoutException)
            {
                return 0;
            }
            catch (RedisConnectionException)
            {
                return 0;
            }
        }

        public virtual async Task HashSetAsync<T>(string key, string field, T value, TimeSpan? absoluteExpiration = null)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _cacheDb.HashSetAsync(key, field, json).ConfigureAwait(false);

            if (absoluteExpiration.HasValue)
                await _cacheDb.KeyExpireAsync(key, absoluteExpiration).ConfigureAwait(false);
        }

        public virtual async Task<T?> HashGetAsync<T>(string key, string field)
        {
            var data = await _cacheDb.HashGetAsync(key, field).ConfigureAwait(false);
            if (!data.HasValue) return default;

            try
            {
                return JsonSerializer.Deserialize<T>(data.ToString()!, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public virtual async Task<bool> HashDeleteFieldAsync(string key, string field)
            => await _cacheDb.HashDeleteAsync(key, field).ConfigureAwait(false);

        public virtual async Task<HashSet<string>> HashGetAllFieldsAsync(string key)
        {
            var entries = await _cacheDb.HashKeysAsync(key).ConfigureAwait(false);
            return entries.Select(e => (string)e).ToHashSet(StringComparer.Ordinal);
        }

        public virtual async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            var hashEntries = await _cacheDb.HashGetAllAsync(key).ConfigureAwait(false);

            var dict = new Dictionary<string, T>(hashEntries.Length, StringComparer.Ordinal);
            foreach (var x in hashEntries)
            {
                try
                {
                    var val = JsonSerializer.Deserialize<T>(x.Value.ToString()!, _jsonOptions);
                    if (val is not null)
                        dict[x.Name.ToString()] = val;
                }
                catch { }
            }

            return dict;
        }

        public virtual async Task<Dictionary<string, T?>> HashGetManyAsync<T>(string key, IEnumerable<string> fields, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(key) || fields is null)
                return new Dictionary<string, T?>(0, StringComparer.Ordinal);

            var arr = fields
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct(StringComparer.Ordinal)
                .Select(f => (RedisValue)f)
                .ToArray();

            if (arr.Length == 0)
                return new Dictionary<string, T?>(0, StringComparer.Ordinal);

            try
            {
                var values = await _cacheDb.HashGetAsync(key, arr).ConfigureAwait(false);

                var dict = new Dictionary<string, T?>(arr.Length, StringComparer.Ordinal);
                for (int i = 0; i < arr.Length; i++)
                {
                    var field = (string)arr[i]!;
                    var rv = values[i];

                    if (!rv.HasValue)
                    {
                        dict[field] = default;
                        continue;
                    }

                    try
                    {
                        dict[field] = JsonSerializer.Deserialize<T>(rv.ToString()!, _jsonOptions);
                    }
                    catch
                    {
                        dict[field] = default;
                    }
                }

                return dict;
            }
            catch (RedisTimeoutException)
            {
                return arr.ToDictionary(f => (string)f!, _ => default(T?), StringComparer.Ordinal);
            }
            catch (RedisConnectionException)
            {
                return arr.ToDictionary(f => (string)f!, _ => default(T?), StringComparer.Ordinal);
            }
        }

        public virtual async Task HashSetManyAsync<T>(string key, IReadOnlyCollection<KeyValuePair<string, T>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(key) || items is null || items.Count == 0)
                return;

            var entries = new List<HashEntry>(items.Count);

            foreach (var kv in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;

                var json = JsonSerializer.Serialize(kv.Value, _jsonOptions);
                entries.Add(new HashEntry(kv.Key, json));
            }

            if (entries.Count == 0)
                return;

            try
            {
                await _cacheDb.HashSetAsync(key, entries.ToArray()).ConfigureAwait(false);

                if (absoluteExpiration.HasValue)
                    await _cacheDb.KeyExpireAsync(key, absoluteExpiration).ConfigureAwait(false);
            }
            catch (RedisTimeoutException)
            {
            }
            catch (RedisConnectionException)
            {
            }
        }

        private bool TryDeserializeEnvelope<T>(string json, out CacheEnvelope<T>? envelope)
        {
            envelope = null;

            if (!json.Contains("\"value\"", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                envelope = JsonSerializer.Deserialize<CacheEnvelope<T>>(json, _jsonOptions);
                return envelope is not null;
            }
            catch
            {
                return false;
            }
        }

        private static TimeSpan ComputeTtl(DateTimeOffset now, DateTimeOffset absoluteExpiresAt, TimeSpan? slidingExpiration)
        {
            var remaining = absoluteExpiresAt - now;
            if (remaining <= TimeSpan.Zero)
                return TimeSpan.FromSeconds(1);

            if (slidingExpiration is null)
                return remaining;

            return remaining <= slidingExpiration.Value ? remaining : slidingExpiration.Value;
        }

        private void TouchSliding<T>(string cacheKey, CacheEnvelope<T> env, DateTimeOffset now)
        {
            if (env.SlidingExpiration is null)
                return;

            var newTtl = env.SlidingExpiration.Value;

            if (env.AbsoluteExpiresAt is not null)
            {
                var remaining = env.AbsoluteExpiresAt.Value - now;
                if (remaining <= TimeSpan.Zero)
                {
                    _ = _cacheDb.KeyDelete(cacheKey);
                    return;
                }

                if (remaining < newTtl)
                    newTtl = remaining;
            }

            if (newTtl > TimeSpan.Zero)
                _ = _cacheDb.KeyExpire(cacheKey, newTtl);
        }

        private async Task TouchSlidingAsync<T>(string cacheKey, CacheEnvelope<T> env, DateTimeOffset now)
        {
            if (env.SlidingExpiration is null)
                return;

            var newTtl = env.SlidingExpiration.Value;

            if (env.AbsoluteExpiresAt is not null)
            {
                var remaining = env.AbsoluteExpiresAt.Value - now;
                if (remaining <= TimeSpan.Zero)
                {
                    _ = _cacheDb.KeyDeleteAsync(cacheKey);
                    return;
                }

                if (remaining < newTtl)
                    newTtl = remaining;
            }

            if (newTtl > TimeSpan.Zero)
                _ = await _cacheDb.KeyExpireAsync(cacheKey, newTtl).ConfigureAwait(false);
        }

        public virtual byte[]? GetBytes(string cacheKey)
        {
            var data = _cacheDb.StringGet(cacheKey);
            if (!data.HasValue) return null;

            return (byte[])data!;
        }

        public virtual async Task<byte[]?> GetBytesAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = await _cacheDb.StringGetAsync(cacheKey).ConfigureAwait(false);
            if (!data.HasValue) return null;

            return (byte[])data!;
        }

        public virtual void SetBytes(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null)
        {
            var ttl = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(1);
            _cacheDb.StringSet(cacheKey, data, ttl);
        }

        public virtual async Task SetBytesAsync(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ttl = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(1);

            await _cacheDb.StringSetAsync(cacheKey, data, ttl).ConfigureAwait(false);
        }

        public virtual async Task HashSetBytesAsync(string key, string field, byte[] value, TimeSpan? absoluteExpiration = null)
        {
            await _cacheDb.HashSetAsync(key, field, value).ConfigureAwait(false);

            if (absoluteExpiration.HasValue)
                await _cacheDb.KeyExpireAsync(key, absoluteExpiration).ConfigureAwait(false);
        }

        public virtual async Task<byte[]?> HashGetBytesAsync(string key, string field)
        {
            var data = await _cacheDb.HashGetAsync(key, field).ConfigureAwait(false);
            if (!data.HasValue) return null;

            return (byte[])data!;
        }

        public virtual async Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key)
        {
            var hashEntries = await _cacheDb.HashGetAllAsync(key).ConfigureAwait(false);

            var dict = new Dictionary<string, byte[]>(hashEntries.Length, StringComparer.Ordinal);
            foreach (var x in hashEntries)
            {
                if (!x.Value.HasValue) continue;
                dict[x.Name.ToString()] = (byte[])x.Value!;
            }

            return dict;
        }

        public virtual async Task<Dictionary<string, byte[]?>> GetBytesManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (keys is null)
                return new Dictionary<string, byte[]?>(0, StringComparer.Ordinal);

            var arr = keys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.Ordinal)
                .Select(k => (RedisKey)k)
                .ToArray();

            if (arr.Length == 0)
                return new Dictionary<string, byte[]?>(0, StringComparer.Ordinal);

            try
            {
                var values = await _cacheDb.StringGetAsync(arr).ConfigureAwait(false);

                var dict = new Dictionary<string, byte[]?>(arr.Length, StringComparer.Ordinal);
                for (int i = 0; i < arr.Length; i++)
                {
                    var v = values[i];
                    dict[(string)arr[i]!] = v.HasValue ? (byte[])v! : null;
                }

                return dict;
            }
            catch (RedisTimeoutException)
            {
                return arr.ToDictionary(k => (string)k!, _ => (byte[]?)null, StringComparer.Ordinal);
            }
            catch (RedisConnectionException)
            {
                return arr.ToDictionary(k => (string)k!, _ => (byte[]?)null, StringComparer.Ordinal);
            }
        }

        public virtual async Task SetBytesManyAsync(IReadOnlyCollection<KeyValuePair<string, byte[]>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (items is null || items.Count == 0)
                return;

            var ttl = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(1);

            var batch = _cacheDb.CreateBatch();
            var tasks = new List<Task>(items.Count);

            foreach (var kv in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var key = kv.Key;
                var val = kv.Value;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (val is null || val.Length == 0)
                    continue;

                tasks.Add(batch.StringSetAsync(key, val, ttl));
            }

            batch.Execute();

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (RedisTimeoutException)
            {
            }
            catch (RedisConnectionException)
            {
            }
        }
    }
}
