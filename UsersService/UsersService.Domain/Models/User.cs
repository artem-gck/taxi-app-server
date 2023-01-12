namespace UsersService.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? CoordinatesId { get; set; }
        public virtual Coordinates? Coordinates { get; set; }
        public virtual Status? Status { get; set; }
    }
}
