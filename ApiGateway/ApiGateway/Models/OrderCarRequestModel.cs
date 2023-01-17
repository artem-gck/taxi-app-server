namespace ApiGateway.Models
{
    public class OrderCarRequestModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DriverId { get; set; }
        public decimal? StartLatitude { get; set; }
        public decimal? StartLongitude { get; set; }
        public decimal? FinishLatitude { get; set; }
        public decimal? FinishLongitude { get; set; }
    }
}
