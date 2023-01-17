namespace Contracts.Shared.FinishTheTransaction
{
    public class SetFinishStatusToOrderResponse
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid DriverId { get; set; }
    }
}
