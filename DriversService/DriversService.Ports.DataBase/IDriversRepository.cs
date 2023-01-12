using DriversService.Domain.Models;

namespace DriversService.Ports.DataBase
{
    public interface IDriversRepository
    {
        public Task<Driver> GetAsync(Guid id);
        public Task UpdateAsync(Guid id, Driver user);
        public Task UpdateStatusAsync(Guid id, string status);
        public Task DeleteAsync(Guid id);
        public Task<Guid> AddAsync(Driver user);
    }
}