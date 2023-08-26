using Microsoft.EntityFrameworkCore;
using Science.Data.Contexts;
using Science.Data.Interfaces;
using Science.Domain.Models;

namespace Science.Data.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext dbContext;

        public BookRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateAsync(Book book)
        {
            await dbContext.Books.AddAsync(book);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await dbContext.Books.ToListAsync();
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            return await dbContext.Books.FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await  dbContext.SaveChangesAsync();
        }
    }
}
