using Ren.Kit.CacheKit.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace Ren.Kit.CacheKit.Services;

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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual string? Get(string cacheKey)
    {
        var value = _cacheDb.StringGet(cacheKey);
        return value.HasValue ? value.ToString() : null;
    }

    /// <inheritdoc/>
    public virtual async Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = await _cacheDb.StringGetAsync(cacheKey).ConfigureAwait(false);
        return value.HasValue ? value.ToString() : null;
    }

    /// <inheritdoc/>
    public virtual void Set<T>(
        string cacheKey,
        T data,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null)
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

    /// <inheritdoc/>
    public virtual async Task SetAsync<T>(
        string cacheKey,
        T data,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
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

    /// <inheritdoc/>
    public virtual void Remove(string cacheKey)
        => _cacheDb.KeyDelete(cacheKey);

    /// <inheritdoc/>
    public virtual async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _cacheDb.KeyDeleteAsync(cacheKey).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
        var server = _connection.GetServer(_connection.GetEndPoints().First());
        server.FlushDatabase(_cacheDb.Database);
    }

    /// <inheritdoc/>
    public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var server = _connection.GetServer(_connection.GetEndPoints().First());
        await server.FlushDatabaseAsync(_cacheDb.Database).ConfigureAwait(false);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual async Task HashSetAsync<T>(
        string key,
        string field,
        T value,
        TimeSpan? absoluteExpiration = null)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _cacheDb.HashSetAsync(key, field, json).ConfigureAwait(false);

        if (absoluteExpiration.HasValue)
            await _cacheDb.KeyExpireAsync(key, absoluteExpiration).ConfigureAwait(false);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual async Task<bool> HashDeleteFieldAsync(string key, string field)
        => await _cacheDb.HashDeleteAsync(key, field).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<HashSet<string>> HashGetAllFieldsAsync(string key)
    {
        var entries = await _cacheDb.HashKeysAsync(key).ConfigureAwait(false);
        return entries.Select(e => (string)e).ToHashSet(StringComparer.Ordinal);
    }

    /// <inheritdoc/>
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

    private static TimeSpan ComputeTtl(
        DateTimeOffset now,
        DateTimeOffset absoluteExpiresAt,
        TimeSpan? slidingExpiration)
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
}
