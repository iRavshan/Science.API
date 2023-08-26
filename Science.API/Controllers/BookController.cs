using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Science.Domain.Models;
using Science.DTO.Book;
using Science.Service.Interfaces;

namespace Science.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService bookService;
        private readonly IMapper mapper;

        public BookController(IBookService bookService,
                              IMapper mapper)
        {
            this.bookService = bookService;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateBook([FromBody] BookCreateDTO book)
        {
            if(ModelState.IsValid)
            {
                Book newBook = mapper.Map<Book>(book); 

                await bookService.CreateAsync(newBook);

                await bookService.SaveChangesAsync();

                return Ok(book);
            }

            return BadRequest(new { error = "Values are not valid" });
        }

        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await bookService.GetAllAsync();

            return Ok(books);
        }

        [HttpGet]
        [Route("getById")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var book = await bookService.GetByIdAsync(id);

            return Ok(book);
        }
    }
}
