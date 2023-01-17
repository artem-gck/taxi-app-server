using Contracts.Shared.StartTripTransaction;
using MassTransit;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Services.Consumers
{
    public class SetProcessingStatusToOrderConsumer : IConsumer<SetProcessingStatusToOrderRequest>
    {
        private readonly IOrdersRepository _ordersRepository;

        public SetProcessingStatusToOrderConsumer(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
        }

        public async Task Consume(ConsumeContext<SetProcessingStatusToOrderRequest> context)
        {
            var orderId = context.Message.OrderId;

            var orderEntity = await _ordersRepository.GetAsync(orderId);

            if (orderEntity.Status.Name == "Waiting")
            {
                await _ordersRepository.UpdateStatusAsync(orderId, "Processing");

                await context.RespondAsync(new SetProcessingStatusToOrderResponse 
                { 
                    OrderId = orderId,
                    UserId = orderEntity.User.Id,
                    DriverId = orderEntity.Driver.Id
                });

                return;
            }

            throw new Exception();
        }
    }
}
