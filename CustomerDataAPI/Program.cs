using DotNetEnv;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CustomerDataService.Data;
using CustomerDataService.Services.Interfaces;
using CustomerDataDomainModels.Models;

// Load environment variables from the .env file
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Will fetch the connection string from .env file(1 arg), if not will give the error message(2 arg):
// string? connectionString = Env.GetString("CONNECTIONSTRING", "Variable not found");
// Retrieve database connection string from the environment variable
string? connectionString = Env.GetString("CONNECTIONSTRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("The connection string is not properly configured!");
}

builder.Services.AddMyAppServices(connectionString); // Use the project service

// Register cascading authentication state for services that need to access authentication state
builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
// Register application services and controllers
// Register services and dependencies for the application
builder.Services.AddControllers(); // Register API controllers

// Retrieve JWT configuration values (Issuer, Audience, and Secret) from the environment
string? issuer = Env.GetString("JWT_ISSUER_DEV");
string? audience = Env.GetString("JWT_AUDIENCE_DEV");
string? secret = Env.GetString("JWT_SECRET");

if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secret))
{
    throw new InvalidOperationException("JWT settings are not properly configured!");
}

// Configure JWT Bearer Authentication for securing the API
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Set token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            // Optional: Adjust clock skew to 0 to prevent timing inconsistencies
            // ClockSkew = TimeSpan.Zero // Optional: helps with token expiration checks
        };

        // Configure token validation events to check for expired or revoked tokens
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Validate token against user data in the database
                IUserService? userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                int userId = int.Parse(context?.Principal?.FindFirst("id")?.Value ?? "0"); // Assuming "id" claim is in JWT

                UserInfo? user = await userService.GetUserByIdAsync(userId);
                if (user is null || user.TokenExpiresAt <= DateTime.UtcNow || user.IsTokenRevoked)
                {
                    // Invalidate the token if it has expired or been revoked
                    context?.Fail("Token has been revoked or expired.");
                }
            }
        };

        // Log any authentication failure details
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token validation failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token valid.");
                return Task.CompletedTask;
            }
        };
    });

// Add support for authorization policies
builder.Services.AddAuthorization();

// Enable Cross-Origin Resource Sharing (CORS) for Blazor app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", builder =>
    {
        builder
            .WithOrigins("http://localhost:7008", "https://localhost:7008")  // Set the origin(port) to match the Blazor app’s address
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Allows cookies or authentication headers
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Swagger / OpenAPI for API documentation (available in both Development and Production environments)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the HTTP request pipeline.
// Configure localization options with "en-US" as the default
CultureInfo? enUs = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(enUs),
    SupportedCultures = new List<CultureInfo> { enUs },
    SupportedUICultures = new List<CultureInfo> { enUs }
};

var app = builder.Build();

app.UseRequestLocalization(localizationOptions); // Enable request localization with specified culture options

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// Conditionally configure Swagger for API documentation based on environment
if (app.Environment.IsDevelopment()) // Swagger is enabled in development
{
    app.UseSwagger();  // Enable middleware to serve generated Swagger as JSON endpoint
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Data API v1");
        c.RoutePrefix = "swagger"; // Set Swagger as the default page
    });
}
else
{
    // Optional: Enable Swagger in production if needed
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Data API v1");
        c.RoutePrefix = "swagger"; // Make Swagger accessible at root in production
    });
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

// Use the CORS policy
app.UseCors("AllowBlazorApp"); // Enable CORS using the defined policy for Blazor app

app.UseAuthentication(); // Enable JWT Authentication Middleware

app.UseAuthorization(); // Enable Authorization Middleware

app.MapControllers(); // Map controller routes to handle API endpoints

app.Run(); // Start the application

