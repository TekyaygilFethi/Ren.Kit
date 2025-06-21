using Microsoft.AspNetCore.Mvc;
using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StandardCacheController(IRENCacheService cacheService) : ControllerBase
{
    /// <summary>
    /// Adds or updates an item in the cache.
    /// </summary>
    [HttpPost("set")]
    public async Task<IActionResult> SetCache([FromQuery] string key, [FromBody] object value)
    {
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(10));
        return Ok("Value cached.");
    }

    /// <summary>
    /// Gets an item from the cache as a specific type.
    /// </summary>
    [HttpGet("get")]
    public async Task<IActionResult> GetCache([FromQuery] string key)
    {
        var value = await cacheService.GetAsync<object>(key);
        if (value is null)
            return NotFound("Key not found in cache.");
        return Ok(value);
    }

    /// <summary>
    /// Gets an item as a string from the cache.
    /// </summary>
    [HttpGet("get-string")]
    public async Task<IActionResult> GetCacheString([FromQuery] string key)
    {
        var value = await cacheService.GetAsync(key);
        if (value is null)
            return NotFound("Key not found in cache.");
        return Ok(value);
    }

    /// <summary>
    /// Removes a key from the cache.
    /// </summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveCache([FromQuery] string key)
    {
        await cacheService.RemoveAsync(key);
        return Ok("Key removed from cache.");
    }

    /// <summary>
    /// Clears all entries in the cache.
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCache()
    {
        await cacheService.ClearAsync();
        return Ok("All cache cleared.");
    }

    /// <summary>
    /// Gets an item from the cache, or creates and caches it if not present.
    /// </summary>
    [HttpGet("get-or-create")]
    public async Task<IActionResult> GetOrCreate([FromQuery] string key)
    {
        var value = await cacheService.GetOrCreateAsync<string>(
            key,
            async ct =>
            {
                // Simulate IO or DB call (return example value)
                await Task.Delay(50, ct);
                return $"Value generated at {DateTime.UtcNow:O}";
            },
            TimeSpan.FromMinutes(10)
        );
        return Ok(value);
    }
}
