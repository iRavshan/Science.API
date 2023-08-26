namespace Science.DTO.Book
{
    public class BookCreateDTO
    {
        public required string ISBN { get; set; }
        public required string Title { get; set; }
        public int YearOfPublication { get; set; }
        public int PublisherId { get; set; }
        public ICollection<int> AuthorsIds { get; set; }
        public string? ImageUrlSmall { get; set; }
        public string? ImageUrlMedium { get; set; }
        public string? ImageUrlLarge { get; set; }
    }
}
