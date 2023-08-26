namespace Science.Domain.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string ISBN { get; set; }
        public required string Title { get; set; }
        public int YearOfPublication { get; set; }
        public Publisher Publisher { get; set; }
        public ICollection<Author> Authors { get; set; }
        public string? ImageUrlSmall { get; set; }
        public string? ImageUrlMedium { get; set; }
        public string? ImageUrlLarge { get; set; }
    }
}
