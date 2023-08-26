using Science.Data.Interfaces;
using Science.Domain.Models;
using Science.Service.Interfaces;

namespace Science.Service.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            this.bookRepository = bookRepository;
        }

        public async Task CreateAsync(Book book)
        {
            await bookRepository.CreateAsync(book);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await bookRepository.GetAllAsync();
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            return await bookRepository.GetByIdAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await bookRepository.SaveChangesAsync();
        }
    }
}
