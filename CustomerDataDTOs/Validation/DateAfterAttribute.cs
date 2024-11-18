using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CustomerDataDTOs.Validation
{
    /// <summary>
    /// Custom validation attribute to check if a date property is after another date property in the same object.
    /// </summary>
    public class DateAfterAttribute : ValidationAttribute
    {
        // The name of the property to compare against.
        private readonly string _comparisonProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateAfterAttribute"/> class with the property to compare against.
        /// </summary>
        /// <param name="comparisonProperty">The name of the property to compare this date to.</param>
        public DateAfterAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        /// <summary>
        /// Validates that the date of the current property is after the date of the comparison property.
        /// </summary>
        /// <param name="value">The value of the property being validated.</param>
        /// <param name="validationContext">Provides context about the object being validated.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the date is valid.</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Cast the value being validated to DateTime, if it exists.
            DateTime? currentValue = (DateTime?)value;

            // Use reflection to get the comparison property by name.
            PropertyInfo? property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            // If the comparison property is not found, throw an exception.
            if (property is null)
                throw new ArgumentException("Property with this name not found");

            // Get the value of the comparison property from the current object instance.
            DateTime? comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            // Perform the validation check:
            // If both dates have values, check if the current value is after the comparison value.
            if (currentValue.HasValue && comparisonValue.HasValue && currentValue.Value < comparisonValue.Value)
            {
                // If the validation fails, return an error message (or a default message if none is provided).
                return new ValidationResult(ErrorMessage ?? "End Date must be after the Start Date");
            }

            // If the validation succeeds, return success.
            return ValidationResult.Success;
        }
    }
}

