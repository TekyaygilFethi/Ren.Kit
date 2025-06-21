using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net8.ExampleAPI.Data.Database;
using Ren.Kit.Net8.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net8.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedAndOverridedDataController(IExtendedRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
{
    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod()
    {
        unitOfWork.AdditionalMethod();
        return Ok("Additional method called successfully.");
    }

    [HttpGet("get-repository")]
    public IActionResult GetRepository(string key)
    {
        unitOfWork.GetRepository<Employee>();
        return Ok();
    }
}