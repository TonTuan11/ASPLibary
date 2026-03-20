using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models;

public class Book
{
    [Key]
    public int BookId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    // FK Author
    public int AuthorId { get; set; }

    [ForeignKey("AuthorId")]
    public Author? Author { get; set; }

    // FK Category
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    public int Stock { get; set; }
}