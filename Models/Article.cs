using System.ComponentModel.DataAnnotations;

namespace Lab06.Models;

public class Article : BaseEntity
{
    [Required]
    [MinLength(5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(20)]
    public string Content { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime PublishedAt { get; set; } = DateTime.Now;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string? AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }

    public string? ImagePath { get; set; }
}
