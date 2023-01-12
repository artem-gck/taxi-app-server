using UsersService.Domain.Models;

namespace UsersService.Application.DataBase
{
    public interface IUsersRepository
    {
        public Task<User> GetAsync(Guid id);
        public Task UpdateAsync(Guid id, User user);
        public Task UpdateStatusAsync(Guid id, string status);
        public Task DeleteAsync(Guid id);
        public Task<Guid> AddAsync(User user);
    }
}