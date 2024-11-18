using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CustomerDataClient;
using CustomerDataClient.Provider;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

// Initialize a new Blazor WebAssembly host builder
WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register root components for the app (primary app and head outlet for setting up metadata like title, etc.)
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register Blazored Local Storage service, used to persist data such as JWT tokens on the client-side
builder.Services.AddBlazoredLocalStorage();

// Configure authorization and authentication
// Register CustomAuthenticationStateProvider to manage authentication states
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

// Set up the AuthenticationStateProvider using the CustomAuthenticationStateProvider instance
// This allows Blazor to use the custom provider for managing the user’s authentication state
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Enable core authorization services for client-side Blazor
// This service enables the use of [Authorize] attribute and other authorization features
builder.Services.AddAuthorizationCore();

//=============================
// Add authorization support
// builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Add custom AuthenticationStateProvider to handle JWT tokens
// Setup HttpClient with API base address
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7280") });
//=============================

// Register CustomAuthorizationMessageHandler to intercept HTTP requests and add the Authorization header when needed
// This custom handler retrieves and attaches the stored JWT token to outgoing requests
builder.Services.AddTransient<CustomAuthorizationMessageHandler>(sp =>
{
    // Retrieve required dependencies: local storage and service provider
    ILocalStorageService? localStorage = sp.GetRequiredService<ILocalStorageService>();
    IServiceProvider? serviceProvider = sp; // This service provider resolve dependencies when needed

    // Return a new instance of CustomAuthorizationMessageHandler with the necessary dependencies
    return new CustomAuthorizationMessageHandler(localStorage, serviceProvider)
    {
        InnerHandler = new HttpClientHandler() // Set the inner handler
    };
});

// Register HttpClient with the custom handler (CustomAuthorizationMessageHandler)
//builder.Services.AddScoped(sp => new HttpClient(sp.GetRequiredService<CustomAuthorizationMessageHandler>())
//{
//    BaseAddress = new Uri("https://localhost:7280")
//});
// Register HttpClient with the CustomAuthorizationMessageHandler

// Register an HttpClient that uses CustomAuthorizationMessageHandler for API requests with JWT token support
builder.Services.AddScoped(sp =>
{
    CustomAuthorizationMessageHandler? handler = sp.GetRequiredService<CustomAuthorizationMessageHandler>();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7280") // The base address for the API endpoints
    };
});

// Run the Blazor application
await builder.Build().RunAsync();

// Enable unencrypted HTTP/2 support for the HttpClient
// Note: Use with caution as unencrypted HTTP/2 connections may expose sensitive information
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);