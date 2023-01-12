namespace DriversService.Domain.Models
{
    public class Status
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public IEnumerable<Driver> Drivers { get; set; }
    }
}
