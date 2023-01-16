namespace Contracts.Shared.StartTripTransaction
{
    public class ProcessCarResponse
    {
        public Guid OrderId { get; set; }

        public string ErrorMessage { get; set; }
    }
}
