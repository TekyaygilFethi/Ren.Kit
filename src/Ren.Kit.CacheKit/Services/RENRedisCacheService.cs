using Microsoft.Extensions.Options;
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
        private bool _useDefaultAbsoluteExpirationWhenNull = true;

        private const bool UseEnvelopeOnlyWhenSlidingProvided = true;

        internal sealed record CacheEnvelope<T>(
            T Value,
            DateTimeOffset CreatedAt,
            DateTimeOffset? AbsoluteExpiresAt,
            TimeSpan? SlidingExpiration
        );

        public RENRedisCacheService(IConnectionMultiplexer connection)
            : this(connection, null)
        {
        }

        public RENRedisCacheService(IConnectionMultiplexer connection, IOptions<RENCacheKitOptions>? options = null)
        {
            _connection = connection;
            _cacheDb = _connection.GetDatabase();

            var opt = options?.Value?.CacheConfiguration;

            if (opt is not null)
            {
                _useDefaultAbsoluteExpirationWhenNull =
                    opt.UseDefaultAbsoluteExpirationWhenNull;

                var timeConfig = opt.RedisConfiguration?.TimeConfiguration;
                if (timeConfig is not null)
                {
                    _defaultAbsoluteExpirationHours =
                        timeConfig.AbsoluteExpirationInHours;
                }
            }
        }

        /// <inheritdoc />
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
                    _cacheDb.KeyDelete(cacheKey);
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

        /// <inheritdoc />
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
                    await _cacheDb.KeyDeleteAsync(cacheKey).ConfigureAwait(false);
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

        /// <inheritdoc />
        public virtual string? Get(string cacheKey)
        {
            var value = _cacheDb.StringGet(cacheKey);
            return value.HasValue ? value.ToString() : null;
        }

        /// <inheritdoc />
        public virtual async Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await _cacheDb.StringGetAsync(cacheKey).ConfigureAwait(false);
            return value.HasValue ? value.ToString() : null;
        }

        /// <inheritdoc />
        public virtual void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var now = DateTimeOffset.UtcNow;

            if (UseEnvelopeOnlyWhenSlidingProvided && slidingExpiration is null)
            {
                var legacyJson = JsonSerializer.Serialize(data, _jsonOptions);
                var ttl = ResolveTtl(absoluteExpiration);
                _ = SetStringCompat(cacheKey, legacyJson, ttl);
                return;
            }

            var abs = ResolveAbsoluteForEnvelope(absoluteExpiration);
            DateTimeOffset? absAt = abs is null ? null : now.Add(abs.Value);

            var env = new CacheEnvelope<T>(data, now, absAt, slidingExpiration);
            var json = JsonSerializer.Serialize(env, _jsonOptions);

            var ttl2 = ComputeEnvelopeTtl(now, absAt, slidingExpiration);
            _ = SetStringCompat(cacheKey, json, ttl2);
        }

        /// <inheritdoc />
        public virtual async Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var now = DateTimeOffset.UtcNow;

            if (UseEnvelopeOnlyWhenSlidingProvided && slidingExpiration is null)
            {
                var legacyJson = JsonSerializer.Serialize(data, _jsonOptions);
                var ttl = ResolveTtl(absoluteExpiration);
                _ = await SetStringCompatAsync(cacheKey, legacyJson, ttl).ConfigureAwait(false);
                return;
            }

            var abs = ResolveAbsoluteForEnvelope(absoluteExpiration);
            DateTimeOffset? absAt = abs is null ? null : now.Add(abs.Value);

            var env = new CacheEnvelope<T>(data, now, absAt, slidingExpiration);
            var json = JsonSerializer.Serialize(env, _jsonOptions);

            var ttl2 = ComputeEnvelopeTtl(now, absAt, slidingExpiration);
            _ = await SetStringCompatAsync(cacheKey, json, ttl2).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual void Remove(string cacheKey)
            => _cacheDb.KeyDelete(cacheKey);

        /// <inheritdoc />
        public virtual async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _cacheDb.KeyDeleteAsync(cacheKey).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual void Clear()
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            server.FlushDatabase(_cacheDb.Database);
        }

        /// <inheritdoc />
        public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            await server.FlushDatabaseAsync(_cacheDb.Database).ConfigureAwait(false);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual async Task<bool> RefreshAsync(string cacheKey, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return false;

            var ttl = ResolveTtl(absoluteExpiration);

            try
            {
                if (ttl is null)
                    return true;

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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual async Task<long> IncrementAsync(string cacheKey, long by = 1, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return 0;

            var ttl = ResolveTtl(absoluteExpiration);

            try
            {
                var next = await _cacheDb.StringIncrementAsync(cacheKey, by).ConfigureAwait(false);

                if (ttl is not null)
                    _ = _cacheDb.KeyExpireAsync(cacheKey, ttl);

                return next;
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

        /// <inheritdoc />
        public virtual async Task HashSetAsync<T>(string key, string field, T value, TimeSpan? absoluteExpiration = null)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _cacheDb.HashSetAsync(key, field, json).ConfigureAwait(false);

            var ttl = ResolveTtl(absoluteExpiration);
            if (ttl is not null)
                await _cacheDb.KeyExpireAsync(key, ttl).ConfigureAwait(false);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual async Task<bool> HashDeleteFieldAsync(string key, string field)
            => await _cacheDb.HashDeleteAsync(key, field).ConfigureAwait(false);

        /// <inheritdoc />
        public virtual async Task<HashSet<string>> HashGetAllFieldsAsync(string key)
        {
            var fields = await _cacheDb.HashKeysAsync(key).ConfigureAwait(false);
            return fields.Select(x => (string)x).ToHashSet(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public virtual async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            var entries = await _cacheDb.HashGetAllAsync(key).ConfigureAwait(false);
            var dict = new Dictionary<string, T>(entries.Length, StringComparer.Ordinal);

            foreach (var e in entries)
            {
                try
                {
                    var v = JsonSerializer.Deserialize<T>(e.Value.ToString()!, _jsonOptions);
                    if (v is not null)
                        dict[e.Name.ToString()] = v;
                }
                catch { }
            }

            return dict;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual async Task HashSetManyAsync<T>(
            string key,
            IReadOnlyCollection<KeyValuePair<string, T>> items,
            TimeSpan? absoluteExpiration = null,
            CancellationToken cancellationToken = default)
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

                var ttl = ResolveTtl(absoluteExpiration);
                if (ttl is not null)
                    await _cacheDb.KeyExpireAsync(key, ttl).ConfigureAwait(false);
            }
            catch (RedisTimeoutException) { }
            catch (RedisConnectionException) { }
        }

        /// <inheritdoc />
        public virtual async Task HashSetBytesAsync(string key, string field, byte[] value, TimeSpan? absoluteExpiration = null)
        {
            await _cacheDb.HashSetAsync(key, field, value).ConfigureAwait(false);

            var ttl = ResolveTtl(absoluteExpiration);
            if (ttl is not null)
                await _cacheDb.KeyExpireAsync(key, ttl).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<byte[]?> HashGetBytesAsync(string key, string field)
        {
            var data = await _cacheDb.HashGetAsync(key, field).ConfigureAwait(false);
            if (!data.HasValue) return null;
            return data;
        }

        /// <inheritdoc />
        public virtual async Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key)
        {
            var entries = await _cacheDb.HashGetAllAsync(key).ConfigureAwait(false);
            var dict = new Dictionary<string, byte[]>(entries.Length, StringComparer.Ordinal);

            foreach (var e in entries)
            {
                if (e.Value.HasValue)
                    dict[e.Name.ToString()] = e.Value;
            }

            return dict;
        }

        /// <inheritdoc />
        public virtual byte[]? GetBytes(string cacheKey)
        {
            var data = _cacheDb.StringGet(cacheKey);
            if (!data.HasValue) return null;
            return data;
        }

        /// <inheritdoc />
        public virtual async Task<byte[]?> GetBytesAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var data = await _cacheDb.StringGetAsync(cacheKey).ConfigureAwait(false);
            if (!data.HasValue) return null;
            return data;
        }

        /// <inheritdoc />
        public virtual void SetBytes(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null)
        {
            var ttl = ResolveTtl(absoluteExpiration);
            _ = SetBytesCompat(cacheKey, data, ttl);
        }

        /// <inheritdoc />
        public virtual async Task SetBytesAsync(string cacheKey, byte[] data, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ttl = ResolveTtl(absoluteExpiration);
            _ = await SetBytesCompatAsync(cacheKey, data, ttl).ConfigureAwait(false);
        }

        /// <inheritdoc />
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
                    dict[(string)arr[i]!] = v.HasValue ? (byte[]?)v : null;
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

        /// <inheritdoc />
        public virtual async Task SetBytesManyAsync(IReadOnlyCollection<KeyValuePair<string, byte[]>> items, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (items is null || items.Count == 0)
                return;

            var ttl = ResolveTtl(absoluteExpiration);

            var tasks = new List<Task>(items.Count);

            foreach (var kv in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(kv.Key))
                    continue;

                if (kv.Value is null || kv.Value.Length == 0)
                    continue;

                tasks.Add(SetBytesCompatAsync(kv.Key, kv.Value, ttl));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (RedisTimeoutException) { }
            catch (RedisConnectionException) { }
        }

        private TimeSpan? ResolveTtl(TimeSpan? absoluteExpiration)
        {
            TimeSpan? ttl =
                absoluteExpiration
                ?? (_useDefaultAbsoluteExpirationWhenNull
                    ? TimeSpan.FromHours(_defaultAbsoluteExpirationHours)
                    : (TimeSpan?)null);

            if (ttl.HasValue && ttl.Value <= TimeSpan.Zero)
                ttl = TimeSpan.FromSeconds(1);

            return ttl;
        }

        private TimeSpan? ResolveAbsoluteForEnvelope(TimeSpan? absoluteExpiration)
        {
            if (absoluteExpiration.HasValue)
                return absoluteExpiration.Value;

            if (_useDefaultAbsoluteExpirationWhenNull)
                return TimeSpan.FromHours(_defaultAbsoluteExpirationHours);

            return null;
        }

        private static TimeSpan? ComputeEnvelopeTtl(DateTimeOffset now, DateTimeOffset? absAt, TimeSpan? sliding)
        {
            if (absAt is null && sliding is null)
                return null;

            if (absAt is null)
                return sliding;

            var remaining = absAt.Value - now;
            if (remaining <= TimeSpan.Zero)
                return TimeSpan.FromSeconds(1);

            if (sliding is null)
                return remaining;

            return remaining <= sliding ? remaining : sliding;
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
                    _cacheDb.KeyDelete(cacheKey);
                    return;
                }

                if (remaining < newTtl)
                    newTtl = remaining;
            }

            if (newTtl > TimeSpan.Zero)
                _cacheDb.KeyExpire(cacheKey, newTtl);
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
                    await _cacheDb.KeyDeleteAsync(cacheKey).ConfigureAwait(false);
                    return;
                }

                if (remaining < newTtl)
                    newTtl = remaining;
            }

            if (newTtl > TimeSpan.Zero)
                await _cacheDb.KeyExpireAsync(cacheKey, newTtl).ConfigureAwait(false);
        }

        private static long ToPxMilliseconds(TimeSpan ttl)
        {
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(1);
            var ms = (long)Math.Ceiling(ttl.TotalMilliseconds);
            return ms <= 0 ? 1 : ms;
        }

        private bool SetStringCompat(string key, string value, TimeSpan? ttl)
        {
            if (ttl is null)
                return _cacheDb.StringSet(key, value);

            var ms = ToPxMilliseconds(ttl.Value);
            var result = _cacheDb.Execute("SET", key, value, "PX", ms);
            return result.ToString() == "OK";
        }

        private async Task<bool> SetStringCompatAsync(string key, string value, TimeSpan? ttl)
        {
            if (ttl is null)
                return await _cacheDb.StringSetAsync(key, value).ConfigureAwait(false);

            var ms = ToPxMilliseconds(ttl.Value);
            var result = await _cacheDb.ExecuteAsync("SET", key, value, "PX", ms).ConfigureAwait(false);
            return result.ToString() == "OK";
        }

        private bool SetBytesCompat(string key, byte[] value, TimeSpan? ttl)
        {
            if (ttl is null)
                return _cacheDb.StringSet(key, value);

            var ms = ToPxMilliseconds(ttl.Value);
            var result = _cacheDb.Execute("SET", key, value, "PX", ms);
            return result.ToString() == "OK";
        }

        private async Task<bool> SetBytesCompatAsync(string key, byte[] value, TimeSpan? ttl)
        {
            if (ttl is null)
                return await _cacheDb.StringSetAsync(key, value).ConfigureAwait(false);

            var ms = ToPxMilliseconds(ttl.Value);
            var result = await _cacheDb.ExecuteAsync("SET", key, value, "PX", ms).ConfigureAwait(false);
            return result.ToString() == "OK";
        }
    }
}
