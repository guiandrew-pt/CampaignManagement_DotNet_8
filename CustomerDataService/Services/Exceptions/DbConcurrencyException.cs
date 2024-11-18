namespace CustomerDataService.Services.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a concurrency conflict occurs during database operations.
    /// </summary>
    /// <remarks>
    /// Use this exception to handle cases where multiple processes or transactions attempt to update 
    /// the same database record simultaneously, resulting in a conflict.
    /// </remarks>
    public class DbConcurrencyException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbConcurrencyException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the concurrency error.</param>
        public DbConcurrencyException(string message) : base(message)
        {
        }
    }
}

