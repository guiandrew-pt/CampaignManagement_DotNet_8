namespace CustomerDataService.Services.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a database operation fails unexpectedly.
    /// </summary>
    /// <remarks>
    /// Use this exception to capture and relay database-specific errors, allowing for both custom error messages 
    /// and the option to include inner exceptions that provide additional context about the error.
    /// </remarks>
    public class DatabaseException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DatabaseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this error.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this error.</param>
        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

