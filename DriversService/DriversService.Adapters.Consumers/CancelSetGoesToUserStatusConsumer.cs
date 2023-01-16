using Contracts.Shared.OrderCarTransaction;
using DriversService.Ports.DataBase;
using MassTransit;

namespace DriversService.Adapters.Consumers
{
    public class CancelSetGoesToUserStatusConsumer : IConsumer<CancelSetGoesToUserDriverStatusRequest>
    {
        private readonly IDriversRepository _driversRepository;

        public CancelSetGoesToUserStatusConsumer(IDriversRepository driversRepository)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetGoesToUserDriverStatusRequest> context)
        {
            var driverId = context.Message.DriverId;

            await _driversRepository.UpdateStatusAsync(driverId, "Free");

            await context.RespondAsync(new SetGoesToUserDriverStatusResponse { DriverId = driverId });
        }
    }
}
