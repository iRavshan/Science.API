using Science.Domain.Models;

namespace Science.Service.Interfaces
{
    public interface IUserAgentService
    {
        Task CreateAsync(UserAgent userAgent);

        Task SaveChangesAsync();
    }
}
