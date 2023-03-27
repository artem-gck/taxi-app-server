using Microsoft.EntityFrameworkCore;
using OrdersService.Access.DataBase.Context;
using OrdersService.Access.DataBase.Entities;
using OrdersService.Access.DataBase.Exceptions;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Access.DataBase.Realisations
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly OrdersContext _ordersContext;

        public OrdersRepository(OrdersContext ordersContext) 
        {
            _ordersContext = ordersContext ?? throw new ArgumentNullException(nameof(ordersContext));
        }

        public async Task<Guid> AddAsync(OrderEntity order)
        {
            order.User = await GetUserAsync(order.User);
            order.Driver = await GetUserAsync(order.Driver);

            var orderEntity = _ordersContext.Orders.Add(order);

            await _ordersContext.SaveChangesAsync();

            return orderEntity.Entity.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var orderEntity = await _ordersContext.Orders
                                                  .Include(or => or.Status)
                                                  .Include(or => or.User)
                                                  .Include(or => or.Driver)
                                                  .Include(or => or.StartCoordinates)
                                                  .Include(or => or.FinishCoordinates)
                                                  .FirstOrDefaultAsync(or => or.Id == id);

            if (orderEntity is null)
                throw new NotFoundOrderException();

            _ordersContext.Orders.Remove(orderEntity);

            await _ordersContext.SaveChangesAsync();
        }

        public async Task<OrderEntity> GetAsync(Guid id)
        {
            var orderEntity = await _ordersContext.Orders
                                                  .Include(or => or.Status)
                                                  .Include(or => or.User)
                                                  .Include(or => or.Driver)
                                                  .Include(or => or.StartCoordinates)
                                                  .Include(or => or.FinishCoordinates)
                                                  .FirstOrDefaultAsync(or => or.Id == id);

            if (orderEntity is null)
                throw new NotFoundOrderException();

            return orderEntity;
        }

        public async Task UpdateAsync(Guid id, OrderEntity order)
        {
            var driverEntity = await _ordersContext.Orders
                                                   .Include(or => or.Status)
                                                   .Include(or => or.User)
                                                   .Include(or => or.Driver)
                                                   .Include(or => or.StartCoordinates)
                                                   .Include(or => or.FinishCoordinates)
                                                   .FirstOrDefaultAsync(or => or.Id == id);

            if (driverEntity is null)
                throw new NotFoundOrderException();

            _ordersContext.Orders.Update(order);

            await _ordersContext.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var orderEntity = await _ordersContext.Orders
                                                  .Include(or => or.Status)
                                                  .Include(or => or.User)
                                                  .Include(or => or.Driver)
                                                  .Include(or => or.StartCoordinates)
                                                  .Include(or => or.FinishCoordinates)
                                                  .FirstOrDefaultAsync(or => or.Id == id);

            if (orderEntity is null)
                throw new NotFoundOrderException();

            var statusEntity = await GetStatusAsync(status);

            orderEntity.Status = statusEntity;

            await _ordersContext.SaveChangesAsync();
        }

        private async Task<StatusEntity> GetStatusAsync(string status)
        {
            var statusEntity = await _ordersContext.Statuses.FirstOrDefaultAsync(st => st.Name == status);

            if (statusEntity is null)
            {
                var statusTrackedEntity = _ordersContext.Statuses.Add(new StatusEntity { Name = status });

                await _ordersContext.SaveChangesAsync();

                statusEntity = statusTrackedEntity.Entity;
            }

            return statusEntity;
        }

        private async Task<UserEntity> GetUserAsync(UserEntity user)
        {
            var userEntity = await _ordersContext.Users.FirstOrDefaultAsync(st => st.Id == user.Id);

            if (userEntity is null)
            {
                var userTrackedEntity = _ordersContext.Users.Add(new UserEntity { Id = user.Id, Name = user.Name, Surname = user.Surname });

                await _ordersContext.SaveChangesAsync();

                userEntity = userTrackedEntity.Entity;
            }

            return userEntity;
        }
    }
}
