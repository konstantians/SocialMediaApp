using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.MVC.CustomValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ComparePasswordAttribute : ValidationAttribute
{
    private readonly string _otherProperty;

    public ComparePasswordAttribute(string otherProperty)
    {
        _otherProperty = otherProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);

        if (otherPropertyInfo == null)
        {
            return new ValidationResult($"Property with name {otherPropertyInfo} not found.");
        }

        var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);

        if (!object.Equals(value, otherValue))
        {
            return new ValidationResult(ErrorMessage ?? "The passwords do not match.");
        }

        return ValidationResult.Success;
    }
}

