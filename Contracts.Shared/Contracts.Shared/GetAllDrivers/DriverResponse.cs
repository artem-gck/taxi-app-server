namespace Contracts.Shared.GetAllDrivers
{
    public class DriverResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public double? Raiting { get; set; }
        public TimeSpan? Experience { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
