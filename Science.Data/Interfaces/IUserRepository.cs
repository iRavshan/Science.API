using Science.Domain.Models;

namespace Science.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(string userId);
        Task SaveChangesAsync();
    }
}
