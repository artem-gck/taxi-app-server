using Contracts.Shared.GetAllDrivers;
using DriversService.Ports.DataBase;
using MassTransit;

namespace DriversService.Adapters.Consumers
{
    public class GetAllDriversConsumer : IConsumer<GetAllDriversRequest>
    {
        private readonly IDriversRepository _driversRepository;

        public GetAllDriversConsumer(IDriversRepository driversRepository)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
        }

        public async Task Consume(ConsumeContext<GetAllDriversRequest> context)
        {
            var status = context.Message.Status;

            var drivers = await _driversRepository.GetAllAsync(status);

            var driversResponse = drivers.Select(dr => new DriverResponse() 
            { 
                Id = dr.Id,
                Name = dr.Name,
                Surname = dr.Surname,
                Raiting = dr.Raiting,
                Experience = TimeSpan.FromTicks(dr.Experience.Value),
                Latitude = dr.Coordinates.Latitude.Value,
                Longitude = dr.Coordinates.Longitude.Value
            }).ToList();

            await context.RespondAsync(new GetAllDriversResponse
            {
                Drivers = driversResponse
            });
        }
    }
}
