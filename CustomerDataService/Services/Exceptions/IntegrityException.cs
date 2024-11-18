namespace CustomerDataService.Services.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a data integrity violation occurs in the application.
    /// </summary>
    /// <remarks>
    /// This exception can be used to handle situations where constraints or integrity rules are broken,
    /// such as violations of foreign key or unique constraints in a database.
    /// </remarks>
    public class IntegrityException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrityException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IntegrityException(string message) : base(message)
        {
        }
    }
}

