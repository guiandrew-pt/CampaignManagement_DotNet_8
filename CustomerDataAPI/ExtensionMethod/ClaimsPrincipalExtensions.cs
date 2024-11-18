using System.Security.Claims;

namespace CustomerDataAPI.ExtensionMethod
{
    /// <summary>
    /// Extension methods for the <see cref="ClaimsPrincipal"/> class, providing additional functionality for retrieving user-specific information from claims.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves the user ID from the claims in a <see cref="ClaimsPrincipal"/> instance.
        /// Assumes the user ID is stored in a claim with the type "id".
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> instance containing user claims.</param>
        /// <returns>
        /// The user ID as an integer, if found and valid; otherwise, <c>null</c> if the claim is missing or invalid.
        /// </returns>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            // Attempt to retrieve the user ID from the "id" claim
            string? userIdClaim = user.FindFirst("id")?.Value;

            // Try to parse the user ID into an integer; return null if parsing fails
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }
    }
}

