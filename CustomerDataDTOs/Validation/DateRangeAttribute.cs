using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CustomerDataDTOs.Validation
{
    /// <summary>
    /// A custom validation attribute that ensures a specified end date is after a start date.
    /// </summary>
    /// <remarks>
    /// This attribute is particularly useful for scenarios where you want to validate a date range,
    /// such as ensuring that the `EndDate` property of a model occurs after the `StartDate` property.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DateRangeAttribute : ValidationAttribute
    {
        /// <summary>
        /// Gets the name of the property representing the start date.
        /// </summary>
        public string StartDateProperty { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRangeAttribute"/> class.
        /// </summary>
        /// <param name="startDateProperty">The name of the property that holds the start date.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="startDateProperty"/> is null or empty.</exception>
        public DateRangeAttribute(string startDateProperty)
        {
            StartDateProperty = startDateProperty;
        }

        /// <summary>
        /// Validates that the end date is after the start date.
        /// </summary>
        /// <param name="value">The value of the property being validated (the end date).</param>
        /// <param name="validationContext">The context in which validation is performed.</param>
        /// <returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Retrieve the property info for the start date
            PropertyInfo? startDateProperty = validationContext.ObjectType.GetProperty(StartDateProperty);
            if (startDateProperty is null)
            {
                return new ValidationResult($"The property '{StartDateProperty}' was not found.");
            }

            // Get the start date value from the object instance
            DateTime? startDate = (DateTime?)startDateProperty?.GetValue(validationContext.ObjectInstance);

            // Validate that the end date is after the start date
            if (value is DateTime endDate && startDate is not null && endDate < startDate)
            {
                return new ValidationResult("End date must be after the start date.");
            }

            return ValidationResult.Success;
        }
    }
}

