using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using CustomerDataClient.Utils;

namespace CustomerDataClient.Provider
{
    /// <summary>
    /// Provides custom authentication state management for Blazor applications, handling JWT-based
    /// user authentication, caching, token storage, and logout functionality.
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
	{
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigation;

        private AuthenticationState? _cachedAuthenticationState = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="httpClient">HttpClient for making authorized API calls.</param>
        /// <param name="localStorage">Service for managing JWT tokens in local storage.</param>
        /// <param name="navigation">Service for handling navigation in the Blazor application.</param>
        public CustomAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage, NavigationManager navigation)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _navigation = navigation;
        }

        /// <summary>
        /// Retrieves the current logged-in user's ID from the authentication state.
        /// </summary>
        /// <returns>The user ID if authenticated; otherwise, null.</returns>
        //public async Task<int?> GetUserIdAsync()
        //{
        //    AuthenticationState? authState = await GetAuthenticationStateAsync();
        //    ClaimsPrincipal? user = authState.User;

        //    if (user.Identity?.IsAuthenticated ?? false)
        //    {
        //        Claim? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        //        if (userIdClaim is not null && int.TryParse(userIdClaim.Value, out int userId))
        //        {
        //            return userId;
        //        }
        //        else
        //        {
        //            Console.WriteLine("User ID claim not found or is invalid.");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("User is not authenticated.");
        //    }

        //    return null; // Return null if the user is not authenticated or the ID claim is unavailable
        //}

        /// <summary>
        /// Retrieves the current <see cref="AuthenticationState"/>, using a cached state if available.
        /// If no cached state is available, it retrieves a JWT token from local storage, validates it,
        /// and constructs a user identity if valid.
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Return cached authentication state if available
            if (_cachedAuthenticationState is not null)
                return _cachedAuthenticationState;

            // Retrieve token from local storage
            string? token = await _localStorage.GetItemAsync<string>("authToken");

            // Console.WriteLine("Retrieved token in AuthenticationStateProvider: " + token); 

            // Console.WriteLine($"Token: {token}"); // Test only

            // Check if the token is missing or expired, logging the user out if necessary
            if (string.IsNullOrWhiteSpace(token) || JwtUtils.IsTokenExpired(token))
            {
                // Console.WriteLine("No valid token, creating anonymous identity."); // Testing only
                // Log the user out if token is missing or expired
                await MarkUserAsLoggedOut();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Anonymous user
            }

            // Console.WriteLine("Token is valid, creating authenticated identity.");

            // Token is valid; Set the Authorization header with the token
            HttpClientUtils.AddDefaultAuthorizationHeader(_httpClient, token); // Testing only

            // Create claims and authentication state
            // Parse claims from JWT and create an authenticated user identity
            IEnumerable<Claim>? claims = ParseClaimsFromJwt(token);
            ClaimsIdentity? identity = new ClaimsIdentity(claims, "jwtAuthType");
            ClaimsPrincipal? user = new ClaimsPrincipal(identity);

            // Log user identity and claims here for debugging
            // Console.WriteLine("User authenticated with claims: " + string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}")));

            _cachedAuthenticationState = new AuthenticationState(user); // Cache the authenticated state
            return _cachedAuthenticationState;

            //return new AuthenticationState(user);
        }

        /// <summary>
        /// Marks the user as authenticated by storing the provided JWT token in local storage,
        /// parsing the token to create a user identity, and caching the authenticated state.
        /// </summary>
        /// <param name="token">The JWT token representing the authenticated user.</param>
        public async Task MarkUserAsAuthenticated(string token)
        {
            // Check if the token is expired before proceeding
            if (JwtUtils.IsTokenExpired(token))
            {
                // If expired, log out the user and skip setting it in local storage
                await MarkUserAsLoggedOut();
                return;
            }

            // Store the valid token in local storage, only if it's valid
            await _localStorage.SetItemAsync("authToken", token);

            // Parse claims and create an authenticated user identity
            // Parse claims and set up user identity, create authenticated user state, and cache it
            IEnumerable<Claim>? claims = ParseClaimsFromJwt(token);
            ClaimsIdentity? identity = new ClaimsIdentity(claims, "jwtAuthType");
            ClaimsPrincipal? user = new ClaimsPrincipal(identity);

            // Cache the authenticated state
            _cachedAuthenticationState = new AuthenticationState(user); // Update cache on login

            // Notify Blazor of the updated authentication state
            // After setting the token to avoid potential async state inconsistencies
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedAuthenticationState));
            //NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        /// <summary>
        /// Marks the user as logged out by removing the JWT token from local storage, clearing the Authorization header,
        /// and resetting the cached authentication state.
        /// </summary>
        public async Task MarkUserAsLoggedOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _cachedAuthenticationState = null; // Clear cached authentication state

            // Clear the cached state
            //_cachedAuthenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Clear cache on logout
            //NotifyAuthenticationStateChanged(Task.FromResult(_cachedAuthenticationState));
            ClaimsPrincipal? anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }

        /// <summary>
        /// Logs the user out by calling an API logout endpoint, removing the JWT token from local storage,
        /// and navigating to the login page.
        /// </summary>
        public async Task LogoutAsync()
        {
            string? token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                HttpClientUtils.AddDefaultAuthorizationHeader(_httpClient, token);
                HttpResponseMessage? response = await _httpClient.PostAsync("api/auth/logout", null);

                if (response.IsSuccessStatusCode)
                {
                    await MarkUserAsLoggedOut(); // This will notify Blazor of the state change
                    // Reload page to clear cached data
                    _navigation.NavigateTo("/login", true); // Clear cached elements the true is optional. It reload the page.
                }
            }
        }

        /// <summary>
        /// Parses claims from the given JWT token to create a list of <see cref="Claim"/> objects.
        /// </summary>
        /// <param name="jwt">The JWT token containing encoded user claims.</param>
        /// <returns>A collection of claims parsed from the JWT token.</returns>
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            // Split the JWT and get the payload part
            string? payload = jwt.Split('.')[1];

            // Decode the payload
            byte[]? jsonBytes = JwtUtils.ParseBase64WithoutPadding(payload);

            try
            {
                // Deserialize into key-value pairs
                Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                // If keyValuePairs is null, return an empty list of claims
                if (keyValuePairs is null)
                    return Enumerable.Empty<Claim>();

                // Return claims ensuring kvp.Key and kvp.Value are not null
                // return keyValuePairs.Select(kvp => new Claim(kvp.Key ?? string.Empty, kvp.Value?.ToString() ?? string.Empty));
                // Map specific claims like username (sub) and email
                // Map specific claims and return a list of claims
                return keyValuePairs.Select(kvp =>
                    kvp.Key switch
                    {
                        "sub" => new Claim(ClaimTypes.Name, kvp.Value?.ToString() ?? string.Empty), // Username claim
                        "email" => new Claim(ClaimTypes.Email, kvp.Value?.ToString() ?? string.Empty), // Email claim
                        // "id" => new Claim(ClaimTypes.NameIdentifier, kvp.Value?.ToString() ?? string.Empty), // User ID claim
                        _ => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty) // Other claims
                    });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JWT claims: {ex.Message}");
                return Enumerable.Empty<Claim>();
            }
        }
    }
}

