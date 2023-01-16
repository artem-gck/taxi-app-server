using DriversService.Adapters.DataBase.Context;
using DriversService.Domain.Exceptions;
using DriversService.Domain.Models;
using DriversService.Ports.DataBase;
using Microsoft.EntityFrameworkCore;

namespace DriversService.Adapters.DataBase
{
    public class DriversRepository : IDriversRepository
    {
        private readonly DriversContext _driversContext;

        public DriversRepository(DriversContext driversContext)
        {
            _driversContext = driversContext ?? throw new ArgumentNullException(nameof(driversContext));
        }

        public async Task<Guid> AddAsync(Driver user)
        {
            var driverEntity = _driversContext.Add(user);

            await _driversContext.SaveChangesAsync();

            return driverEntity.Entity.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var driverEntity = await _driversContext.Drivers
                                                    .Include(dr => dr.Coordinates)
                                                    .Include(dr => dr.Status)
                                                    .FirstOrDefaultAsync(us => us.Id == id);

            if (driverEntity is null)
                throw new NotFoundDriverException();

            _driversContext.Drivers.Remove(driverEntity);

            await _driversContext.SaveChangesAsync();
        }

        public async Task<Driver> GetAsync(Guid id)
        {
            var driverEntity = await _driversContext.Drivers
                                                    .Include(dr => dr.Coordinates)
                                                    .Include(dr => dr.Status)
                                                    .FirstOrDefaultAsync(us => us.Id == id);

            if (driverEntity is null)
                throw new NotFoundDriverException();

            return driverEntity;
        }

        public async Task UpdateAsync(Guid id, Driver user)
        {
            var driverEntity = await _driversContext.Drivers
                                                    .Include(dr => dr.Coordinates)
                                                    .Include(dr => dr.Status)
                                                    .FirstOrDefaultAsync(us => us.Id == id);

            if (driverEntity is null)
                throw new NotFoundDriverException();

            _driversContext.Drivers.Update(user);

            await _driversContext.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var driverEntity = await _driversContext.Drivers
                                                    .Include(dr => dr.Coordinates)
                                                    .Include(dr => dr.Status)
                                                    .FirstOrDefaultAsync(us => us.Id == id);

            if (driverEntity is null)
                throw new NotFoundDriverException();

            var statusEntity = await GetStatusAsync(status);

            driverEntity.Status = statusEntity;

            await _driversContext.SaveChangesAsync();
        }

        private async Task<Status> GetStatusAsync(string status)
        {
            var statusEntity = await _driversContext.Statuses.FirstOrDefaultAsync(st => st.Name == status);

            if (statusEntity is null)
            {
                var statusTrackedEntity = _driversContext.Statuses.Add(new Status { Name = status });

                await _driversContext.SaveChangesAsync();

                statusEntity = statusTrackedEntity.Entity;
            }

            return statusEntity;
        }
    }
}