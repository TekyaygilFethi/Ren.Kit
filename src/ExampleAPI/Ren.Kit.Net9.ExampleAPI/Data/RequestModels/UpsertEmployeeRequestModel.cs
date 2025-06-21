namespace Ren.Kit.Net9.ExampleAPI.Data.RequestModels
{
    public class UpsertEmployeeRequestModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public Guid DepartmentId { get; set; }
    }
}
