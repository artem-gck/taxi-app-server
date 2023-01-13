namespace OrdersService.Access.DataBase.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public UserEntity? User { get; set; }
        public UserEntity? Driver { get; set; }
        public CoordinatesEntity? StartCoordinates { get; set; }
        public CoordinatesEntity? FinishCoordinates { get; set; }
        public decimal? Price { get; set; }
        public double? Duration { get; set; }
        public double? Distance { get; set; }
        public StatusEntity? Status { get; set; }
    }
}
