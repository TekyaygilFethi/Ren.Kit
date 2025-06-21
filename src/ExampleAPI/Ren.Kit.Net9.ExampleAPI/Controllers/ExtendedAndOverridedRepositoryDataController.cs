using Microsoft.AspNetCore.Mvc;
using Ren.Kit.Net9.ExampleAPI.Customizations.Extend.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Customizations.ExtendAndOverride.Services;
using Ren.Kit.Net9.ExampleAPI.Data.Database;
using Ren.Kit.Net9.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExtendedAndOverridedRepositoryDataController(IExtendedRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
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