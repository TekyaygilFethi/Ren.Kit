using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class ExtendedInMemoryCacheController(IExtendedRENInMemoryCacheService cacheService) : ControllerBase
{
    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod(string key)
    {
        cacheService.AdditionalMethod();
        return Ok();
    }
}
