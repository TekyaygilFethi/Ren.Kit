namespace Ren.Kit.Net9.ExampleAPI.Data.Entities;

public class Department
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ICollection<Employee> Employees { get; set; }
}
