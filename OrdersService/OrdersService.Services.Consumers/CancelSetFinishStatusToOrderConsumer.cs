using Contracts.Shared.FinishTheTransaction;
using MassTransit;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Services.Consumers
{
    public class CancelSetFinishStatusToOrderConsumer : IConsumer<CancelSetFinishStatusToOrderRequest>
    {
        private readonly IOrdersRepository _ordersRepository;

        public CancelSetFinishStatusToOrderConsumer(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetFinishStatusToOrderRequest> context)
        {
            var orderMessage = context.Message;

            var orderEntity = await _ordersRepository.GetAsync(orderMessage.OrderId);

            if (orderEntity.Status.Name == "Finish")
            {
                await _ordersRepository.UpdateStatusAsync(orderMessage.OrderId, "Processing");

                orderEntity = await _ordersRepository.GetAsync(orderMessage.OrderId);

                orderEntity.Duration = null;
                orderEntity.Distance = null;
                orderEntity.Price = null;

                await _ordersRepository.UpdateAsync(orderMessage.OrderId, orderEntity);

                await context.RespondAsync(new CancelSetFinishStatusToOrderResponse
                {
                    OrderId = orderMessage.OrderId
                });

                return;
            }

            throw new Exception();
        }
    }
}
