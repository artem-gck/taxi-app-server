using Microsoft.EntityFrameworkCore;
using UsersService.Domain.Models;

namespace UsersService.Infrastructure.DataBase.Context
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Coordinates> Coordinates { get; set; }
        public DbSet<Status> Statuses { get; set; }

        public UsersContext(DbContextOptions<UsersContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Database.EnsureCreated();
        }
    }
}
