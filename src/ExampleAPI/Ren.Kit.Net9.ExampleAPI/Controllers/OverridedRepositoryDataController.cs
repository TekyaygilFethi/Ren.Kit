using Microsoft.AspNetCore.Mvc;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Customizations.Override.Services;
using Ren.Kit.Net9.ExampleAPI.Data.Database;
using Ren.Kit.Net9.ExampleAPI.Data.Entities;

namespace Ren.Kit.Net9.ExampleAPI.Controllers
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
