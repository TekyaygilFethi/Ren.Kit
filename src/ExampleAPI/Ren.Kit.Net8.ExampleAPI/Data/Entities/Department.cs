﻿namespace Ren.Kit.Net8.ExampleAPI.Data.Entities;

public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<Employee> Employees { get; set; }
}
