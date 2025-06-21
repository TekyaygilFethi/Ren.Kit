using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

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