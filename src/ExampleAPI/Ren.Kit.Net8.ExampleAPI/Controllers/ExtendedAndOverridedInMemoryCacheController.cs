using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedAndOverridedInMemoryCacheController(IExtendedRENInMemoryCacheService cacheService) : ControllerBase
{
    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod(string key)
    {
        cacheService.AdditionalMethod();
        return Ok();
    }

    [HttpPost("set")]
    public async Task<IActionResult> SetCache([FromQuery] string key, [FromBody] object value)
    {
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(10));
        return Ok("Value cached.");
    }
}