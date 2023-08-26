namespace Science.DTO.Book
{
    public class BookCreateDTO
    {
        public required string Title { get; set; }
        public double Price { get; set; }
        public int Avilability { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Stars { get; set; }
    }
}
