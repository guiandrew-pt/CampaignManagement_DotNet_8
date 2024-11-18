namespace CustomerDataClient.Models.Validation
{
    /// <summary>
    /// Represents a paginated response containing a subset of data, along with pagination details.
    /// </summary>
    /// <typeparam name="T">The type of data contained in the paginated response.</typeparam>
    public class PaginatedResponse<T> // (Generic) 
	{
        /// <summary>
        /// Gets the list of items for the current page.
        /// </summary>
        public List<T> Data { get; }

        /// <summary>
        /// Gets the current page number (1-based index).
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Gets the number of items per page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of records in the data set.
        /// </summary>
        public int TotalRecords { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class.
        /// </summary>
        /// <param name="data">The list of data items for the current page.</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="totalRecords">The total number of records in the complete data set.</param>
        public PaginatedResponse(List<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }
    }
}

