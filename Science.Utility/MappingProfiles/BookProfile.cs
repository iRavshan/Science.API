using AutoMapper;
using Science.Domain.Models;
using Science.DTO.Book;

namespace Science.Utility.MappingProfiles
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<BookCreateDTO, Book>();
        }
    }
}
