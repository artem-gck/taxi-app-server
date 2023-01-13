using Contracts.Shared;
using MassTransit;
using OrdersService.Access.DataBase.Entities;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Services.Consumers
{
    public class AddNewOrderConsumer : IConsumer<IAddOrderRequest>
    {
        private readonly IOrdersRepository _ordersRepository;

        public AddNewOrderConsumer(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
        }

        public async Task Consume(ConsumeContext<IAddOrderRequest> context)
        {
            var order = new OrderEntity()
            {
                User = new UserEntity()
                {
                    Id = context.Message.UserId,
                    Name = context.Message.UserName,
                    Surname = context.Message.UserSurname,
                },
                Driver = new UserEntity()
                {
                    Id = context.Message.DriverId,
                    Name = context.Message.DriverName,
                    Surname = context.Message.DriverSurname,
                },
                StartCoordinates = new CoordinatesEntity()
                {
                    Latitude = context.Message.StartLatitude,
                    Longitude = context.Message.StartLongitude,
                },
                FinishCoordinates = new CoordinatesEntity()
                {
                    Latitude = context.Message.FinishLatitude,
                    Longitude = context.Message.FinishLongitude,
                },
                Status = new StatusEntity()
                {
                    Name = "Waiting"
                }
            };

            var orderId = await _ordersRepository.AddAsync(order);
            await context.RespondAsync<IAddOrderResponse>(new { orderId });
        }
    }
}
