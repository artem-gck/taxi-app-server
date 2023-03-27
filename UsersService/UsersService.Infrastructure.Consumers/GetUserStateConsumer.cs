using Contracts.Shared.GetUserState;
using MassTransit;
using UsersService.Application.DataBase;

namespace UsersService.Infrastructure.Consumers
{
    public class GetUserStateConsumer : IConsumer<GetUserStateRequest>
    {
        private readonly IUsersRepository _userRepository;

        public GetUserStateConsumer(IUsersRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task Consume(ConsumeContext<GetUserStateRequest> context)
        {
            var userId = context.Message.UserId;

            var user = await _userRepository.GetAsync(userId);

            await context.RespondAsync(new GetUserStateResponse
            {
                State = user.Status.Name
            });
        }
    }
}
