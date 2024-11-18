namespace CustomerDataService.Services.Exceptions
{
    /// <summary>
    /// Exception class to represent cases where a requested resource is not found.
    /// Inherits from <see cref="ApplicationException"/> to provide a custom message for not found errors.
    /// </summary>
    public class NotFoundException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotFoundException(string message) : base(message)
        {
        }
    }
}

