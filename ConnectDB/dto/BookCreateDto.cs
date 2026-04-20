namespace ConnectDB.dto
{
    public class BookCreateDto
    {
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public int Stock { get; set; }
    }
}
