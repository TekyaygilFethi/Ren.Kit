using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ren.Kit.DataKit.Abstractions;
using Ren.Kit.Net9.ExampleAPI.Data.Database;
using Ren.Kit.Net9.ExampleAPI.Data.Entities;
using Ren.Kit.Net9.ExampleAPI.Data.RequestModels;

namespace Ren.Kit.Net9.ExampleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StandardDataController(IRENUnitOfWork<RenDbContext> unitOfWork) : ControllerBase
{
    // Post: api/Data/Departments/create-mock
    [HttpPost("departments/create-mock")]
    public async Task<IActionResult> GetDepartmentsFiltered([FromQuery] Guid id, [FromQuery] string name)
    {
        var newDepartment = new Department
        {
            Id = id,
            Name = name
        };

        unitOfWork.GetRepository<Department>().Insert(newDepartment);
        await unitOfWork.SaveChangesAsync();
        return Ok(newDepartment);
    }

    // GET: api/Data/Departments/filtered?name=Engineering
    [HttpGet("departments/filtered")]
    public async Task<IActionResult> GetDepartmentsFiltered([FromQuery] string? name)
    {
        // Example: Filtering, ordering, including, readonly
        var departments = await unitOfWork.GetRepository<Department>()
            .GetListAsync(
                filter: d => name == null || d.Name.Contains(name),
                orderBy: q => q.OrderByDescending(d => d.Name),
                include: q => q.Include(d => d.Employees),
                isReadOnly: true
            );
        return Ok(departments);
    }

    // GET: api/Data/Employees/ordered
    [HttpGet("employees/ordered")]
    public async Task<IActionResult> GetEmployeesOrdered()
    {
        var employees = await unitOfWork.GetRepository<Employee>()
            .GetListAsync(
                orderBy: q => q.OrderBy(e => e.Salary),
                isReadOnly: true
            );
        return Ok(employees);
    }

    // GET: api/Data/Employee/single?id={id}
    [HttpGet("employee/single")]
    public async Task<IActionResult> GetSingleEmployee([FromQuery] Guid id)
    {
        var employee = await unitOfWork.GetRepository<Employee>()
            .GetSingleAsync(
                filter: e => e.Id == id,
                include: q => q.Include(e => e.Department),
                isReadOnly: true
            );
        return Ok(employee);
    }

    // POST: api/Data/Employees/BulkInsert
    [HttpPost("employees/bulk-insert")]
    public async Task<IActionResult> BulkInsertEmployees([FromBody] List<UpsertEmployeeRequestModel> employees)
    {
        unitOfWork.GetRepository<Employee>().BulkInsert(employees.Select(_ => new Employee { Id = _.Id, Name = _.Name, Salary = _.Salary, DepartmentId = _.DepartmentId }));
        await unitOfWork.SaveChangesAsync();
        return Ok("Bulk insert completed");
    }

    // POST: api/Data/DoComplexTransaction
    [HttpPost("doComplexTransaction")]
    public async Task<IActionResult> DoComplexTransaction([FromBody] List<UpsertEmployeeRequestModel> employees)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            unitOfWork.GetRepository<Employee>().BulkInsert(employees.Select(_ => new Employee { Id = _.Id, Name = _.Name, Salary = _.Salary, DepartmentId = _.DepartmentId }));
            await unitOfWork.SaveChangesAsync();

            // Other codes....
            // _unitOfWork.GetRepository<Department>().Update(...);

            await unitOfWork.CommitTransactionAsync();
            return Ok("Transaction successful");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return BadRequest(ex.Message);
        }
    }

    // GET: api/Data/Employees/advanced-query
    [HttpGet("employees/advanced-query")]
    public async Task<IActionResult> AdvancedQuery()
    {
        var employees = await unitOfWork.GetRepository<Employee>()
            .GetListAsync(
                filter: e => e.Salary > 50000,
                orderBy: q => q.OrderByDescending(e => e.Salary),
                include: q => q.Include(e => e.Department),
                isReadOnly: true
            );
        return Ok(employees);
    }

    // POST: api/Data/Employees/Upsert
    [HttpPost("employees/upsert")]
    public async Task<IActionResult> UpsertEmployee([FromBody] UpsertEmployeeRequestModel employee)
    {
        var repo = unitOfWork.GetRepository<Employee>();
        var exists = await repo.AnyAsync(e => e.Id == employee.Id);
        var entity = new Employee
        {
            Id = employee.Id,
            Name = employee.Name,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };

        if (exists)
            repo.Update(entity);
        else
            repo.Insert(entity);

        await unitOfWork.SaveChangesAsync();
        return Ok();
    }


    // DELETE: api/Data/Employees/BulkDelete
    [HttpDelete("employees/bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] List<Guid> employeeIds)
    {
        var repo = unitOfWork.GetRepository<Employee>();
        var toDelete = await repo.GetListAsync(e => employeeIds.Contains(e.Id));
        repo.Delete(toDelete);
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}
