using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CustomerDataService.Data
{
    /// <summary>
    /// Factory class for creating instances of <see cref="CustomerDataContext"/> at design time.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IDesignTimeDbContextFactory{TContext}"/> interface,
    /// enabling the creation of a <see cref="CustomerDataContext"/> during design-time operations,
    /// such as applying migrations via the EF Core CLI or tools.
    /// </remarks>
    public class CustomerDataContextFactory : IDesignTimeDbContextFactory<CustomerDataContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="CustomerDataContext"/> using the provided arguments.
        /// </summary>
        /// <param name="args">Arguments passed by the tooling, typically not used in this implementation.</param>
        /// <returns>A configured instance of <see cref="CustomerDataContext"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the connection string is not found or is improperly configured in the environment file.
        /// </exception>
        public CustomerDataContext CreateDbContext(string[] args)
        {
            // Load environment variables explicitly for design-time support
            Env.TraversePath().Load();

            // Retrieve connection string from configuration
            string? connectionString = Env.GetString("CONNECTIONSTRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string is not properly configured in .env!");
            }

            // Configure DbContext options with MySQL provider
            DbContextOptionsBuilder<CustomerDataContext>? optionsBuilder = new DbContextOptionsBuilder<CustomerDataContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new CustomerDataContext(optionsBuilder.Options);
        }
    }
}

