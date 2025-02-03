using System.ComponentModel.DataAnnotations;

namespace receipt.processor.Models;

public record Item(
    [Required] string ShortDescription, 
    [Required] [RegularExpression(@"^\d+\.\d{2}$")]decimal Price);