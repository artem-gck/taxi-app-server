namespace Contracts.Shared
{
    public interface IAddOrderRequest
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public Guid DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverSurname { get; set; }
        public decimal? StartLatitude { get; set; }
        public decimal? StartLongitude { get; set; }
        public decimal? FinishLatitude { get; set; }
        public decimal? FinishLongitude { get; set; }
    }
}
