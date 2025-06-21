using Microsoft.AspNetCore.Mvc;
using Ren.Kit.CacheKit.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverridedCacheController(IRENCacheService cacheService) : ControllerBase
{
    [HttpGet("get")]
    public async Task<IActionResult> GetCacheValueAsync(string key)
    {
        var value = await cacheService.GetAsync<string>(key);
        return Ok(value);
    }
}
