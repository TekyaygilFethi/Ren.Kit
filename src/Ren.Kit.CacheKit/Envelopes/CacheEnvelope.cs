namespace Ren.Kit.CacheKit.Envelopes;

internal sealed record CacheEnvelope<T>(
    T Value,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AbsoluteExpiresAt,
    TimeSpan? SlidingExpiration
);

