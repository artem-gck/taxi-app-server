using Microsoft.EntityFrameworkCore;
using OrdersService.Access.DataBase.Entities;

namespace OrdersService.Access.DataBase.Context
{
    public class OrdersContext : DbContext
    {
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<CoordinatesEntity> Coordinates { get; set; }
        public DbSet<StatusEntity> Statuses { get; set; }

        public OrdersContext(DbContextOptions<OrdersContext> options)
            : base(options)
        { }
    }
}
