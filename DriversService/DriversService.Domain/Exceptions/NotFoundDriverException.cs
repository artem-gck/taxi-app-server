namespace DriversService.Domain.Exceptions
{
    public class NotFoundDriverException : Exception
    {
        public NotFoundDriverException() 
            : base("Not found driver") { }
    }
}
