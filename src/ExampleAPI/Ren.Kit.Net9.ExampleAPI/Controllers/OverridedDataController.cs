using Microsoft.AspNetCore.Mvc;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Data.Database;
using Ren.Kit.Net9.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverridedDataController(IRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
{
    [HttpGet("get-repository")]
    public IActionResult GetRepository(string key)
    {
        unitOfWork.GetRepository<Employee>();
        return Ok();
    }
}
