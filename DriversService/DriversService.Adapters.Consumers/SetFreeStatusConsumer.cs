using Contracts.Shared.FinishTheTransaction;
using DriversService.Ports.DataBase;
using MassTransit;

namespace DriversService.Adapters.Consumers
{
    public class SetFreeStatusConsumer : IConsumer<SetFreeStatusToDriverRequest>
    {
        private readonly IDriversRepository _driversRepository;

        public SetFreeStatusConsumer(IDriversRepository driversRepository)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
        }

        public async Task Consume(ConsumeContext<SetFreeStatusToDriverRequest> context)
        {
            var driverId = context.Message.DriverId;

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
