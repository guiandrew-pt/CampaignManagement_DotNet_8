using Blazored.LocalStorage;
using CustomerDataClient.Models.ViewModels;
using CustomerDataClient.Provider;
using CustomerDataClient.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

//// This custom message handler is designed to automatically add an Authorization header with a JWT token to outgoing HTTP requests.
//public class CustomAuthorizationMessageHandler : DelegatingHandler
//{
//    // Dependency Injection: Uses the ILocalStorageService to retrieve the JWT token from local storage.
//    private readonly ILocalStorageService _localStorage;

//    // Constructor injecting ILocalStorageService for managing local storage operations.
//    public CustomAuthorizationMessageHandler(ILocalStorageService localStorage)
//    {
//        _localStorage = localStorage;
//    }

//    // Overrides the SendAsync method to intercept outgoing HTTP requests.
//    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//    {
//        // Retrieve the token from local storage.
//        string? token = await _localStorage.GetItemAsync<string>("authToken");

//        // If a token is available, add it to the Authorization header.
//        if (!string.IsNullOrEmpty(token))
//        {
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//        }

//        // Proceed with the original request, passing it along to the next handler in the pipeline.
//        return await base.SendAsync(request, cancellationToken);
//    }
//}

/// <summary>
/// Custom message handler that automatically attaches a JWT token to outgoing HTTP requests for authorization purposes.
/// This handler retrieves the token from local storage and validates its expiration before adding it to the request header(Authorization header).
/// </summary>
public class CustomAuthorizationMessageHandler : DelegatingHandler
{
    // Dependency Injection fields: Uses the ILocalStorageService to retrieve the JWT token from local storage.
    private readonly ILocalStorageService _localStorage;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomAuthorizationMessageHandler"/> class.
    /// </summary>
    /// <param name="localStorage">Service for accessing local storage to retrieve and manage the JWT token.</param>
    /// <param name="serviceProvider">Service provider to resolve dependencies, particularly for handling expired tokens.</param>
    public CustomAuthorizationMessageHandler(ILocalStorageService localStorage, IServiceProvider serviceProvider)
    {
        _localStorage = localStorage;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Intercepts and processes outgoing HTTP requests to add an Authorization header with the JWT token, if available and valid.
    /// Overrides the SendAsync method to intercept outgoing HTTP requests.
    /// </summary>
    /// <param name="request">The HTTP request message being processed.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response message from the server.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Retrieve the token from local storage.
        string? token = await _localStorage.GetItemAsync<string>("authToken");

        // Add the token to the Authorization header if it exists and is not expired
        if (!string.IsNullOrEmpty(token) && !JwtUtils.IsTokenExpired(token))
        {
            HttpClientUtils.AddAuthorizationHeader(request, token); // Add token to Authorization header
        }
        else
        {
            // Console.WriteLine("Token expired or missing. Handling expiration."); // Testing only
            // Handle expired or missing token by marking the user as logged out
            await HandleTokenExpirationAsync();
        }

        // Proceed with the original request, passing it along to the next handler in the pipeline.
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Marks the user as logged out by invoking the <see cref="CustomAuthenticationStateProvider"/>, which removes the token from local storage
    /// and notifies the application of the updated authentication state.
    /// </summary>
    private async Task HandleTokenExpirationAsync()
    {
        // Resolve the authentication state provider to manage the logout process if the token has expired
        // Lazy resolve `AuthenticationStateProvider` only if the token is expired
        CustomAuthenticationStateProvider? authProvider = (CustomAuthenticationStateProvider)_serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        // CustomAuthenticationStateProvider? authProvider = _serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>();
        await authProvider.MarkUserAsLoggedOut();
    }
}