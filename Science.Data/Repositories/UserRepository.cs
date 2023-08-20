using Microsoft.EntityFrameworkCore;
using Science.Data.Contexts;
using Science.Data.Interfaces;
using Science.Domain.Models;

namespace Science.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext dbContext;

        public UserRepository(AppDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await dbContext.Users.Include(user => user.UserAgents).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string userId)
        {
            return await dbContext.Users.Include(user => user.UserAgents).FirstOrDefaultAsync(w => w.Id == userId);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
