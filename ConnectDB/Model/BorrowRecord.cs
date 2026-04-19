using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models;

public class BorrowRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BorrowId { get; set; }

    // FK Member
    public int MemberId { get; set; }

    [ForeignKey("MemberId")]
    public Member? Member { get; set; }

    // FK Book
    public int BookId { get; set; }

    [ForeignKey("BookId")]
    public Book? Book { get; set; }

    public DateTime BorrowDate { get; set; } = DateTime.Now;

    public DateTime? ReturnDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Borrowing"; // Borrowing / Returned
}