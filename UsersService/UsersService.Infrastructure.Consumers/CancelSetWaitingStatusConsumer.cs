using Contracts.Shared.OrderCarTransaction;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Infrastructure.Consumers
{
    public class CancelSetWaitingStatusConsumer : IConsumer<CancelSetWaitingUserStatusRequest>
    {
        private readonly IUsersRepository _userRepository;

        public CancelSetWaitingStatusConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetWaitingUserStatusRequest> context)
        {
            var userId = context.Message.UserId;

            await _userRepository.UpdateStatusAsync(userId, "Free");

            await context.RespondAsync(new CancelSetWaitingUserStatusResponse { UserId = userId });
        }
    }
}
