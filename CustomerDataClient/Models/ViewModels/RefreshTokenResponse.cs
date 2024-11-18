namespace CustomerDataClient.Models.ViewModels
{
    /// <summary>
    /// Model to represent the response received from the token refresh endpoint.
    /// This typically includes a new access token and, optionally, an updated refresh token.
    /// </summary>
    public class RefreshTokenResponse
    {
        /// <summary>
        /// Gets or sets the new access token.
        /// This token is used for authenticating subsequent API requests.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the new refresh token.
        /// This is optional and may be returned by the server 
        /// if a new refresh token is issued upon refresh.
        /// </summary>
        public string? RefreshToken { get; set; } 
    }
}

