using Contracts.Shared.FinishTheTransaction;
using MassTransit;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Services.Consumers
{
    public class SetFinishStatusToOrderConsumer : IConsumer<SetFinishStatusToOrderRequest>
    {
        private readonly IOrdersRepository _ordersRepository;

        public SetFinishStatusToOrderConsumer(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
        }

        public async Task Consume(ConsumeContext<SetFinishStatusToOrderRequest> context)
        {
            var orderMessage = context.Message;

            var orderEntity = await _ordersRepository.GetAsync(orderMessage.OrderId);

            if (orderEntity.Status.Name == "Processing")
            {
                await _ordersRepository.UpdateStatusAsync(orderMessage.OrderId, "Finish");

                orderEntity = await _ordersRepository.GetAsync(orderMessage.OrderId);

                orderEntity.Duration = orderMessage.Duration;
                orderEntity.Distance = orderMessage.Distance;
                orderEntity.Price = orderMessage.Price;

                await _ordersRepository.UpdateAsync(orderMessage.OrderId, orderEntity);

                await context.RespondAsync(new SetFinishStatusToOrderResponse
                {
                    OrderId = orderMessage.OrderId,
                    UserId = orderEntity.User.Id,
                    DriverId = orderEntity.Driver.Id
                });

                return;
            }

            throw new Exception();
        }
    }
}
