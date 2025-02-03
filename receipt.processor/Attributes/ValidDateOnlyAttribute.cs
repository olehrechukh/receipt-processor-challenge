using System.ComponentModel.DataAnnotations;

namespace receipt.processor.Attributes;

public class ValidDateOnlyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string str || DateOnly.TryParse(str, out _))
            return ValidationResult.Success;

        return new ValidationResult("Invalid time format. Please provide a valid time.");
    }
}