using Contracts.Shared.OrderCarTransaction;
using Contracts.Shared.StartTripTransaction;
using DriversService.Ports.DataBase;
using MassTransit;

namespace DriversService.Adapters.Consumers
{
    public class CancelSetOnTheTripStatusConsumer : IConsumer<CancelSetOnTheTripStatusRequest>
    {
        private readonly IDriversRepository _driversRepository;

        public CancelSetOnTheTripStatusConsumer(IDriversRepository driversRepository)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetOnTheTripStatusRequest> context)
        {
            var driverId = context.Message.DriverId;

            await _driversRepository.UpdateStatusAsync(driverId, "Goes to the user");

            await context.RespondAsync(new CancelSetOnTheTripStatusResponse { DriverId = driverId });
        }
    }
}
