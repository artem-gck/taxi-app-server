namespace DriversService.Domain.Models
{
    public class Driver
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsOnline { get; set; }
        public Status? Status { get; set; }
        public Coordinates? Coordinates { get; set; }
        public double? Raiting { get; set; }
        public int? CountOfReview { get; set; }
        public TimeSpan? Experience { get; set; }
    }
}
