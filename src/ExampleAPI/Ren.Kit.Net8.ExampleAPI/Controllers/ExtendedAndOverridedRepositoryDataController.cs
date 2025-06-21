using Microsoft.AspNetCore.Mvc;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.Net8.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net8.ExampleAPI.Customizations.ExtendAndOverride.Services;
using Ren.Kit.Net8.ExampleAPI.Data.Database;
using Ren.Kit.Net8.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net8.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedAndOverridedRepositoryDataController(IRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
{
    private readonly IExtendedRENRepository<Employee> customEmployeeRepository = unitOfWork.GetRepository<ExtendedAndOverridedRENRepository<Employee>, Employee>();

    [HttpGet("additional-method")]
    public IActionResult AdditionalMethod()
    {
        customEmployeeRepository.AdditionalMethod();
        return Ok("Additional method called successfully.");
    }

    [HttpGet("get-employees")]
    public IActionResult GetRepository()
    {
        var employees = customEmployeeRepository.GetList();
        return Ok(employees);
    }
}