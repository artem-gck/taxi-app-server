namespace UsersService.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() 
            : base("User not found") { }
    }
}
