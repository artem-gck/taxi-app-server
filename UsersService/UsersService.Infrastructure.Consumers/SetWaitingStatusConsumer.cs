using Contracts.Shared.OrderCarTransaction;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Application.Consumers
{
    public class SetWaitingStatusConsumer : IConsumer<SetWaitingUserStatusRequest>
    {
        private readonly IUsersRepository _userRepository;

        public SetWaitingStatusConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<SetWaitingUserStatusRequest> context)
        {
            var userId = context.Message.UserId;

            var user = await _userRepository.GetAsync(userId);

            if (user.Status?.Name == "Free")
            {
                await _userRepository.UpdateStatusAsync(userId, "Waiting car");
                await context.RespondAsync(new SetWaitingUserStatusResponse { UserId = userId });

                return;
            }

            throw new Exception();
        }
    }
}