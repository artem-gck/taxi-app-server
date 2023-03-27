using Contracts.Shared.FinishTheTransaction;
using DriversService.Ports.DataBase;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DriversService.Adapters.Consumers
{
    public class SetFreeStatusConsumer : IConsumer<SetFreeStatusToDriverRequest>
    {
        private readonly IDriversRepository _driversRepository;
        private readonly ILogger<SetFreeStatusConsumer> _logger;

        public SetFreeStatusConsumer(IDriversRepository driversRepository, ILogger<SetFreeStatusConsumer> logger)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SetFreeStatusToDriverRequest> context)
        {
            _logger.LogCritical("Start SetFreeStatusConsumer");

            var driverId = context.Message.DriverId;

            _logger.LogCritical(driverId.ToString());

            var user = await _driversRepository.GetAsync(driverId);

            if (user.Status?.Name == "On the trip" && user.IsOnline.Value)
            {
                await _driversRepository.UpdateStatusAsync(driverId, "Free");
                await context.RespondAsync(new SetFreeStatusToDriverResponse
                {
                    DriverId = driverId
                });

                return;
            }

            throw new Exception();
        }
    }
}
