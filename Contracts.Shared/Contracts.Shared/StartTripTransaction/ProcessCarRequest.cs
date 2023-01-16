namespace Contracts.Shared.StartTripTransaction
{
    public class ProcessCarRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DriverId { get; set; }
        public Guid OrderId { get; set; }
    }
}
