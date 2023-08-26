namespace Science.Domain.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public double Price { get; set; }
        public int? Avilability { get; set; }
        public int? NumberOfReviews { get; set; }
        public ICollection<Category> Categories { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? Stars { get; set; }
    }
}
