using Microsoft.EntityFrameworkCore;
using UsersService.Application.DataBase;
using UsersService.Domain.Exceptions;
using UsersService.Domain.Models;
using UsersService.Infrastructure.DataBase.Context;

namespace UsersService.Infrastructure.DataBase
{
    public class UsersRepository : IUsersRepository
    {
        private readonly UsersContext _usersContext;

        public UsersRepository(UsersContext usersContext) 
        {
            _usersContext = usersContext ?? throw new ArgumentNullException(nameof(usersContext));
        }

        public async Task<Guid> AddAsync(User user)
        {
            var userEntity = _usersContext.Add(user);

            await _usersContext.SaveChangesAsync();

            return userEntity.Entity.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var userEntity = await _usersContext.Users.FindAsync(id);

            if (userEntity is null)
                throw new NotFoundException();

            _usersContext.Users.Remove(userEntity);

            await _usersContext.SaveChangesAsync();
        }

        public async Task<User> GetAsync(Guid id)
        {
            var userEntity = await _usersContext.Users.FindAsync(id);

            if (userEntity is null)
                throw new NotFoundException();

            return userEntity;
        }

        public async Task UpdateAsync(Guid id, User user)
        {
            var userEntity = await _usersContext.Users.FindAsync(id);

            if (userEntity is null)
                throw new NotFoundException();

            _usersContext.Users.Update(user);

            await _usersContext.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var userEntity = await _usersContext.Users.FindAsync(id);

            if (userEntity is null)
                throw new NotFoundException();

            var statusEntity = await GetStatusAsync(status);

            userEntity.Status = statusEntity;

            await _usersContext.SaveChangesAsync();
        }

        private async Task<Status> GetStatusAsync(string status)
        {
            var statusEntity = await _usersContext.Statuses.FirstOrDefaultAsync(st => st.Name == status);

            if (statusEntity is null)
            {
                var statusTrackedEntity = _usersContext.Statuses.Add(new Status { Name = status });

                await _usersContext.SaveChangesAsync();

                statusEntity = statusTrackedEntity.Entity;
            }

            return statusEntity;
        }
    }
}