using Science.Domain.Models;

namespace Science.Service.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetByIdAsync(string userId);
        Task SaveChangesAsync();
    }
}
