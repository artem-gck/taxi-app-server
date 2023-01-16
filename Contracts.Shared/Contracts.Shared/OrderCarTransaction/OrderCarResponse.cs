namespace Contracts.Shared.OrderCarTransaction
{
    public class OrderCarResponse
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
