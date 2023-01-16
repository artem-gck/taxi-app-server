namespace ApiGateway.Models
{
    public class ProcessCarRequestModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DriverId { get; set; }
        public Guid OrderId { get; set; }
    }
}
