namespace ApiGateway.Models
{
    public class FinishCarRequestModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal? Price { get; set; }
        public double? Duration { get; set; }
        public double? Distance { get; set; }
    }
}
