namespace OrdersService.Access.DataBase.Entities
{
    public class CoordinatesEntity
    {
        public Guid Id { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
