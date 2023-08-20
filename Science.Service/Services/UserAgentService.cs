using Science.Data.Interfaces;
using Science.Domain.Models;
using Science.Service.Interfaces;

namespace Science.Service.Services
{
    public class UserAgentService : IUserAgentService
    {
        private readonly IUserAgentRepository userAgentRepository;

        public UserAgentService(IUserAgentRepository userAgentRepository)
        {
            this.userAgentRepository = userAgentRepository;
        }

        public async Task CreateAsync(UserAgent userAgent)
        {
            await userAgentRepository.CreateAsync(userAgent);
        }

        public async Task SaveChangesAsync()
        {
            await userAgentRepository.SaveChangesAsync();
        }
    }
}
