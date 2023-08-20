using Science.Data.Interfaces;
using Science.Domain.Models;
using Science.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Science.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository) 
        {
            this.userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<User> GetByIdAsync(string userId)
        {
            return await userRepository.GetByIdAsync(userId);
        }

        public async Task SaveChangesAsync()
        {
            await userRepository.SaveChangesAsync();
        }
    }
}
