namespace CustomerDataAPI.ViewModels
{
    /// <summary>
    /// A generic class that encapsulates paginated data and metadata about the pagination state,
    /// such as the current page, page size, and total record count.
    /// </summary>
    /// <typeparam name="T">The type of the data elements in the paginated response.</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Gets or sets the collection of data items for the current page.
        /// </summary>
        public List<T> Data { get; set; }

        /// <summary>
        /// Gets or sets the current page number in the paginated response.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of records per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of records across all pages.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class with
        /// the specified data, page number, page size, and total record count.
        /// </summary>
        /// <param name="data">The list of data items for the current page.</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="totalRecords">The total number of records across all pages.</param>
        public PaginatedResponse(List<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }
    }
}

