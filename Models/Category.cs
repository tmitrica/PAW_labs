using System.ComponentModel.DataAnnotations;

namespace Lab06.Models;

public class Category : BaseEntity
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    public List<Article> Articles { get; set; } = [];
}
