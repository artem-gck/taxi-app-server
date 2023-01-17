using Contracts.Shared.FinishTheTransaction;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Infrastructure.Consumers
{
    public class SetFreeStatusConsumer :  IConsumer<SetFreeStatusToUserRequest>
    {
        private readonly IUsersRepository _userRepository;

        public SetFreeStatusConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<SetFreeStatusToUserRequest> context)
        {
            var userId = context.Message.UserId;

            var user = await _userRepository.GetAsync(userId);

            if (user.Status?.Name == "On the trip")
            {
                await _userRepository.UpdateStatusAsync(userId, "Free");
                await context.RespondAsync(new SetFreeStatusToUserResponse
                {
                    UserId = userId
                });

                return;
            }

            throw new Exception();
        }
    }
}
