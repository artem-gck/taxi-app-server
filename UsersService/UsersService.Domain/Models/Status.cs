namespace UsersService.Domain.Models
{
    public class Status
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public virtual IEnumerable<User>? Users { get; set; }
    }
}
