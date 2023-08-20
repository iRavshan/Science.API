using Science.Data.Contexts;
using Science.Data.Interfaces;
using Science.Domain.Models;

namespace Science.Data.Repositories
{
    public class UserAgentRepository : IUserAgentRepository
    {
        private readonly AppDbContext dbContext;

        public UserAgentRepository(AppDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }

        public async Task CreateAsync(UserAgent userAgent)
        {
            await dbContext.UserAgents.AddAsync(userAgent);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
