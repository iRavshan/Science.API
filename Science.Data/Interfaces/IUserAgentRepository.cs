using Science.Domain.Models;

namespace Science.Data.Interfaces
{
    public interface IUserAgentRepository
    {
        Task CreateAsync(UserAgent userAgent);
        Task SaveChangesAsync();
    }
}
