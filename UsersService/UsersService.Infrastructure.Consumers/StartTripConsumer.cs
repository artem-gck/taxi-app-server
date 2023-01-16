using MassTransit;
using Contracts.Shared.StartTripTransaction;
using UsersService.Application.DataBase;

namespace UsersService.Infrastructure.Consumers
{
    public class StartTripConsumer : IConsumer<SetOnTheTripUserStatusRequest>
    {
        private readonly IUsersRepository _userRepository;

        public StartTripConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<SetOnTheTripUserStatusRequest> context)
        {
            var userId = context.Message.UserId;

            var user = await _userRepository.GetAsync(userId);

            if (user.Status?.Name == "Waiting car")
            {
                await _userRepository.UpdateStatusAsync(userId, "On the trip");
                await context.RespondAsync(new SetOnTheTripUserStatusResponse { UserId = userId });

                return;
            }

            throw new Exception();
        }
    }
}
