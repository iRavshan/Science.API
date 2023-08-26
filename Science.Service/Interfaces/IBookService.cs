using Science.Domain.Models;

namespace Science.Service.Interfaces
{
    public interface IBookService
    {
        Task<Book> GetByIdAsync(int id);
        Task<IEnumerable<Book>> GetAllAsync();
        Task CreateAsync(Book book);
        Task SaveChangesAsync();
    }
}
