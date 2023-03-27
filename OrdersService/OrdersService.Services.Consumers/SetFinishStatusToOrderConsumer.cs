using Contracts.Shared.FinishTheTransaction;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrdersService.Access.DataBase.Interfaces;

namespace OrdersService.Services.Consumers
{
    public class SetFinishStatusToOrderConsumer : IConsumer<SetFinishStatusToOrderRequest>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILogger<SetFinishStatusToOrderConsumer> _logger;

        public SetFinishStatusToOrderConsumer(IOrdersRepository ordersRepository, ILogger<SetFinishStatusToOrderConsumer> logger)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SetFinishStatusToOrderRequest> context)
        {
            _logger.LogCritical("Start SetFinishStatusToOrderConsumer");

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

                _logger.LogCritical(orderMessage.OrderId.ToString());
                _logger.LogCritical(orderEntity.User.Id.ToString());
                _logger.LogCritical(orderEntity.Driver.Id.ToString());

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
