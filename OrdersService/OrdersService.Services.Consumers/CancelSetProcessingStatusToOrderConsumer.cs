using Contracts.Shared.StartTripTransaction;
using MassTransit;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Services.Consumers
{
    public class CancelSetProcessingStatusToOrderConsumer : IConsumer<CancelSetProcessingStatusToOrderRequest>
    {
        private readonly IOrdersRepository _ordersRepository;

        public CancelSetProcessingStatusToOrderConsumer(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetProcessingStatusToOrderRequest> context)
        {
            var orderId = context.Message.OrderId;

            var orderEntity = await _ordersRepository.GetAsync(orderId);

            if (orderEntity.Status.Name == "Processing")
            {
                await _ordersRepository.UpdateStatusAsync(orderId, "Waiting");

                await context.RespondAsync(new CancelSetProcessingStatusToOrderResponse
                {
                    OrderId = orderId
                });

                return;
            }

            throw new Exception();
        }
    }
}
