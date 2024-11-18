namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for handling the response of an authentication request.
    /// Contains the JWT token and its expiration time.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Gets or sets the JSON Web Token (JWT) issued upon successful authentication.
        /// This token is used for authenticating subsequent requests.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiration date and time of the token.
        /// Indicates when the token will become invalid and a new token will be required.
        /// </summary>
        public DateTime Expires { get; set; }
    }
}

