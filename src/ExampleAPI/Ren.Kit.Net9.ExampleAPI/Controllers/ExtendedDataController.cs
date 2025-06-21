using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Data.Database;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedDataController(IExtendedRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
{
    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod()
    {
        unitOfWork.AdditionalMethod();
        return Ok("Additional method called successfully.");
    }
}
