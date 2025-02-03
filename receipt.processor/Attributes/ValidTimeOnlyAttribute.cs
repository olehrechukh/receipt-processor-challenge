using System.ComponentModel.DataAnnotations;

namespace receipt.processor.Attributes;

public class ValidTimeOnlyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string str || TimeOnly.TryParse(str, out _))
            return ValidationResult.Success;

        return new ValidationResult("Invalid time format. Please provide a valid time.");
    }
}