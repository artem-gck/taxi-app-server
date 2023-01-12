using Contracts.Shared;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Application.Consumers
{
    public class SetWaitingStatusConsumer : IConsumer<ISetWaitingUserStatusRequest>
    {
        private readonly IUsersRepository _userRepository;

        public SetWaitingStatusConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<ISetWaitingUserStatusRequest> context)
        {
            var userId = context.Message.UserId;

            var user = await _userRepository.GetAsync(userId);

            if (user.Status?.Name == "Free")
            {
                await _userRepository.UpdateStatusAsync(userId, "Waiting car");
                await context.RespondAsync<ISetWaitingUserStatusResponse>(new { context.Message.UserId });
            }
        }
    }
}