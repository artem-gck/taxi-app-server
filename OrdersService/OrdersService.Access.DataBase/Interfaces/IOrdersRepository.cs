using OrdersService.Access.DataBase.Entities;

namespace OrdersService.Access.DataBase.Interfaces
{
    public interface IOrdersRepository
    {
        public Task<OrderEntity> GetAsync(Guid id);
        public Task UpdateAsync(Guid id, OrderEntity order);
        public Task UpdateStatusAsync(Guid id, string status);
        public Task DeleteAsync(Guid id);
        public Task<Guid> AddAsync(OrderEntity user);
    }
}
