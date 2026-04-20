namespace ConnectDB.dto
{
    public class BookUpdateDto
    {
        public string? Title { get; set; }
        public int? AuthorId { get; set; }
        public int? CategoryId { get; set; }
        public int? Stock { get; set; }
    }
}
