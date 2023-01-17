using Contracts.Shared.FinishTheTransaction;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Infrastructure.Consumers
{
    public class CancelSetFreeStatusConsumer : IConsumer<CancelSetFreeStatusToUserRequest>
    {
        private readonly IUsersRepository _userRepository;

        public CancelSetFreeStatusConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<CancelSetFreeStatusToUserRequest> context)
        {
            var userId = context.Message.UserId;

            var user = await _userRepository.GetAsync(userId);

            if (user.Status?.Name == "Free")
            {
                await _userRepository.UpdateStatusAsync(userId, "On the trip");
                await context.RespondAsync(new CancelSetFreeStatusToUserResponse
                {
                    UserId = userId
                });

                return;
            }

            throw new Exception();
        }
    }
}
