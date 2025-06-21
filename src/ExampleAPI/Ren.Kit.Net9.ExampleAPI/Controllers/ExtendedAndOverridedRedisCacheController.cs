using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedAndOverridedRedisCacheController(IExtendedRENRedisCacheService cacheService) : ControllerBase
{
    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod(string key)
    {
        cacheService.AdditionalMethod();
        return Ok();
    }

    [HttpPost("set")]
    public IActionResult SetCache([FromQuery] string key, [FromBody] object value)
    {
        cacheService.Set(key, value, TimeSpan.FromMinutes(10));
        return Ok("Value cached.");
    }
}