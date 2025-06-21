using Microsoft.AspNetCore.Mvc;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.Net8.ExampleAPI.Customizations.Override.Services;
using Ren.Kit.Net8.ExampleAPI.Data.Database;
using Ren.Kit.Net8.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net8.ExampleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OverridedRepositoryDataController(IRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
    {
        private readonly IRENRepository<Employee> customEmployeeRepository = unitOfWork.GetRepository<OverridedRENRepository<Employee>, Employee>();

        [HttpGet("find")]
        public IActionResult GetRepository(Guid key)
        {
            var employee = customEmployeeRepository.Find(key);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
    }
}
