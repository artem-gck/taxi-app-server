using Contracts.Shared.StartTripTransaction;
using DriversService.Ports.DataBase;
using MassTransit;

namespace DriversService.Adapters.Consumers
{
    public class SetOnTheTripStatusConsumer : IConsumer<SetOnTheTripStatusRequest>
    {
        private readonly IDriversRepository _driversRepository;

        public SetOnTheTripStatusConsumer(IDriversRepository driversRepository)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
        }

        public async Task Consume(ConsumeContext<SetOnTheTripStatusRequest> context)
        {
            var driverId = context.Message.DriverId;

            var user = await _driversRepository.GetAsync(driverId);

            if (user.Status?.Name == "Goes to the user" && user.IsOnline.Value)
            {
                await _driversRepository.UpdateStatusAsync(driverId, "On the trip");
                await context.RespondAsync(new SetOnTheTripStatusResponse { DriverId = driverId });

                return;
            }

            throw new Exception();
        }
    }
}
