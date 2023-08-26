using Science.Domain.Models;

namespace Science.Data.Interfaces
{
    public interface IBookRepository
    {
        Task<Book> GetByIdAsync(int id);
        Task<IEnumerable<Book>> GetAllAsync();
        Task CreateAsync(Book book);
        Task SaveChangesAsync();
    }
}
