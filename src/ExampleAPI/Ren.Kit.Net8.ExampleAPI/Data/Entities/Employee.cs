namespace Ren.Kit.Net8.ExampleAPI.Data.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; }
}