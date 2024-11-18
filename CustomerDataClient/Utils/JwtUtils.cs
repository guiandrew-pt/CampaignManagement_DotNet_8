using System.Text.Json;

namespace CustomerDataClient.Utils
{
    /// <summary>
    /// Utility class for handling JWT (JSON Web Token) operations, 
    /// such as checking token expiration and decoding base64 payloads.
    /// </summary>
    public static class JwtUtils
	{
        /// <summary>
        /// Determines whether a JWT token has expired by decoding its payload and reading the "exp" (expiration) claim.
        /// </summary>
        /// <param name="token">The JWT token to be checked.</param>
        /// <returns>
        /// True if the token is expired or invalid; otherwise, false if the token is still valid.
        /// </returns>
        public static bool IsTokenExpired(string token)
        {
            // Return true if the token is null or empty, indicating it is expired or invalid.
            if (string.IsNullOrEmpty(token))
            {
                // Console.WriteLine("Token is null or empty. Assuming expired."); // It's only for testing
                return true;
            }

            try
            {
                // Decode JWT payload (second part of the JWT format)
                string payload = token.Split('.')[1];
                byte[] jsonBytes = ParseBase64WithoutPadding(payload);

                // Deserialize the payload JSON into a key-value dictionary
                Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                // Check if the payload contains an "exp" claim, which stores expiration time as Unix timestamp
                if (keyValuePairs is not null && keyValuePairs.TryGetValue("exp", out object? expValue))
                {
                    long expUnixTime = 0;

                    // Handle expValue as JSON element or directly as Int64
                    // Check the "exp" claim format and parse its value accordingly
                    if (expValue is JsonElement jsonElement && jsonElement.ValueKind is JsonValueKind.Number)
                    {
                        expUnixTime = jsonElement.GetInt64();
                    }
                    else if (expValue is long longExpValue)
                    {
                        expUnixTime = longExpValue;
                    }
                    else
                    {
                        // Console.WriteLine("Unexpected 'exp' format in token."); // It's only for testing
                        // If "exp" format is unrecognized, return true to indicate expired or invalid token
                        return true;
                    }

                    // Convert Unix timestamp to DateTime in UTC and compare with current UTC time
                    DateTime expirationTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;
                    // Console.WriteLine("Token expiration time: " + expirationTime); // It's only for testing
                    // Console.WriteLine("Current time: " + DateTime.UtcNow); // It's only for testing

                    return expirationTime < DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                // Log the exception and assume the token is expired if any errors occur
                Console.WriteLine($"Error checking token expiration: {ex.Message}");
            }

            return true; // Assume expired if there is an error or no "exp" claim
        }

        /// <summary>
        /// Decodes a base64-encoded string while handling padding issues.
        /// This method is primarily used to decode JWT payloads.
        /// </summary>
        /// <param name="base64">The base64-encoded string to be decoded.</param>
        /// <returns>A byte array representing the decoded data.</returns>
        public static byte[] ParseBase64WithoutPadding(string base64)
        {
            // Ensure base64 string has proper padding by adding '=' as needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            // Decode the base64 string and return as byte array
            return Convert.FromBase64String(base64);
        }
    }
}

