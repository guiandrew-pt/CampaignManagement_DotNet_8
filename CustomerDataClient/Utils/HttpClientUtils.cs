using System.Net.Http.Headers;

namespace CustomerDataClient.Utils
{
    /// <summary>
    /// Utility class for handling authorization headers in HTTP requests with a JWT token.
    /// </summary>
    public static class HttpClientUtils
    {
        /// <summary>
        /// Adds an Authorization header with a Bearer token to a specific HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request to which the header is added.</param>
        /// <param name="token">The JWT token to be added to the Authorization header.</param>
        public static void AddAuthorizationHeader(HttpRequestMessage request, string token)
        {
            // Check if the token is non-empty or null
            if (!string.IsNullOrEmpty(token))
            {
                // Assign the Authorization header using Bearer token authentication
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Sets a default Authorization header with a Bearer token on the provided HttpClient instance.
        /// This header will be included in all outgoing requests made with this HttpClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to which the default header is added.</param>
        /// <param name="token">The JWT token to be added to the Authorization header.</param>
        public static void AddDefaultAuthorizationHeader(HttpClient httpClient, string token)
        {
            // Check if the token is non-empty or null
            if (!string.IsNullOrEmpty(token))
            {
                // Set a default Authorization header for all requests using this HttpClient instance
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}

