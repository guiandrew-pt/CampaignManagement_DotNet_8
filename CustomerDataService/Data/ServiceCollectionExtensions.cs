using CustomerDataService.Services;
using CustomerDataService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerDataService.Data
{
    /// <summary>
    /// Contains extension methods for configuring application services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the application's services and configures the database context using the provided connection string.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
        /// <param name="connectionString">The connection string used to configure the database context.</param>
        /// <returns>
        public static IServiceCollection AddMyAppServices(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string is not properly configured!");
            }

            // Configure DbContext to use MySQL, with automatic migration detection for 'CustomerDataContext'
            services.AddDbContext<CustomerDataContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                builder => builder.MigrationsAssembly("CustomerDataSerivce")));

            // Register the services
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<ICustomerDataService, Services.CustomerDataService>();
            services.AddScoped<ISendsEmailsService, SendsEmailsService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}

