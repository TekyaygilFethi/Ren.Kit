using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net8.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedRedisCacheController(IExtendedRENRedisCacheService cacheService) : ControllerBase
{
    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod(string key)
    {
        cacheService.AdditionalMethod();
        return Ok();
    }
}