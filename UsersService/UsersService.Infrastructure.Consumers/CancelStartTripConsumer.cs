using Contracts.Shared.OrderCarTransaction;
using Contracts.Shared.StartTripTransaction;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Infrastructure.Consumers
{
    public class CancelStartTripConsumer : IConsumer<CancelSetOnTheTripUserStatusRequest>
    {
        private readonly IUsersRepository _userRepository;

        public CancelStartTripConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetOnTheTripUserStatusRequest> context)
        {
            var userId = context.Message.UserId;

            await _userRepository.UpdateStatusAsync(userId, "Waiting car");

            await context.RespondAsync(new CancelSetOnTheTripUserStatusResponse{ UserId = userId });
        }
    }
}
