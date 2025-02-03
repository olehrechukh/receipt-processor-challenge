using System.ComponentModel.DataAnnotations;
using receipt.processor.Attributes;

namespace receipt.processor.Models;

public record ReceiptDto(
    [Required]
    [RegularExpression(@"^[\w\s\-&]+$")]
    string Retailer,
    [Required] [ValidDateOnly] string PurchaseDate,
    [Required] [ValidTimeOnly] string PurchaseTime,
    [Required] [MinLength(1)] List<Item> Items,
    [Required]
    [RegularExpression(@"^\d+\.\d{2}$")]
    decimal Total)
{
    public Receipt ToReceipt()
    {
        return new Receipt(Retailer, DateOnly.Parse(PurchaseDate), TimeOnly.Parse(PurchaseTime), Items, Total);
    }
}