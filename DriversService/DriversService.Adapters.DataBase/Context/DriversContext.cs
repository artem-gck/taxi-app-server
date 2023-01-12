using DriversService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DriversService.Adapters.DataBase.Context
{
    public class DriversContext : DbContext
    {
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Coordinates> Coordinates { get; set; }
        public DbSet<Status> Statuses { get; set; }

        public DriversContext(DbContextOptions<DriversContext> options)
            : base(options)
        { }
    }
}
