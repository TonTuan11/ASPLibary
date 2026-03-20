using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Author
{
    [Key]
    public int AuthorId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}