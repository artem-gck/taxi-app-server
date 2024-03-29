﻿using Contracts.Shared.GetAllDrivers;
using Contracts.Shared.OrderCarTransaction;
using DriversService.Ports.DataBase;
using MassTransit;

namespace DriversService.Adapters.Consumers
{
    public class SetGoesToUserStatusConsumer : IConsumer<SetGoesToUserDriverStatusRequest>
    {
        private readonly IDriversRepository _driversRepository;

        public SetGoesToUserStatusConsumer(IDriversRepository driversRepository)
        {
            _driversRepository = driversRepository ?? throw new ArgumentNullException(nameof(driversRepository));
        }

        public async Task Consume(ConsumeContext<SetGoesToUserDriverStatusRequest> context)
        {
            var driverId = context.Message.DriverId;

            var user = await _driversRepository.GetAsync(driverId);

            if (user.Status?.Name == "Free" && user.IsOnline.Value)
            {
                await _driversRepository.UpdateStatusAsync(driverId, "Goes to the user");
                await context.RespondAsync(new SetGoesToUserDriverStatusResponse 
                { 
                    DriverId = driverId,
                    Name = user.Name,
                    Surname = user.Surname
                });

                return;
            }

            throw new Exception();
        }
    }
}