﻿using Ren.Kit.CacheKit.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace Ren.Kit.CacheKit.Services;


/// <summary>
/// Redis-based, async-compatible cache service implementing IRENCacheService.
/// </summary>
public class RENRedisCacheService : IRENCacheService
{
    private readonly IDatabase _cacheDb;
    private readonly IConnectionMultiplexer _connection;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private int _defaultAbsoluteExpirationHours = 12;

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
        try { return JsonSerializer.Deserialize<T>(data, _jsonOptions); }
        catch { return default; }
    }

    /// <inheritdoc/>
    public virtual async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await _cacheDb.StringGetAsync(cacheKey);
        if (!data.HasValue) return default;
        try { return JsonSerializer.Deserialize<T>(data, _jsonOptions); }
        catch { return default; }
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
        var value = await _cacheDb.StringGetAsync(cacheKey);
        return value.HasValue ? value.ToString() : null;
    }

    /// <inheritdoc/>
    public virtual void Set<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        // Sliding expiration not natively supported in Redis
        var expiration = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        _cacheDb.StringSet(cacheKey, json, expiration);
    }

    /// <inheritdoc/>
    public virtual async Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var expiration = absoluteExpiration ?? TimeSpan.FromHours(_defaultAbsoluteExpirationHours);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await _cacheDb.StringSetAsync(cacheKey, json, expiration);
    }

    /// <inheritdoc/>
    public virtual void Remove(string cacheKey)
    {
        _cacheDb.KeyDelete(cacheKey);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _cacheDb.KeyDeleteAsync(cacheKey);
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
        await server.FlushDatabaseAsync(_cacheDb.Database);
    }

    /// <inheritdoc/>
    public virtual async Task<T?> GetOrCreateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var value = await factory(cancellationToken);
        if (value is not null)
            await SetAsync(cacheKey, value, absoluteExpiration, slidingExpiration, cancellationToken);

        return value;
    }
}
