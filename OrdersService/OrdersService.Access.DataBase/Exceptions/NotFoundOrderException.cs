namespace OrdersService.Access.DataBase.Exceptions
{
    public class NotFoundOrderException : Exception
    {
        public NotFoundOrderException() 
        : base("Order not found") { }
    }
}
