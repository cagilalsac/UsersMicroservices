using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Users.APP.Domain
{
    /// <summary>
    /// Provides a factory for creating <see cref="UsersDb"/> instances at design time.
    /// This is used by Entity Framework Core tools (such as migrations) to construct the database context
    /// when the application is not running, typically using configuration from appsettings.json.
    /// This class should be created if there are any exceptions during scaffolding.
    /// </summary>
    public class UsersDbFactory : IDesignTimeDbContextFactory<UsersDb>
    {
        /// <summary>
        /// The name of the connection string in the configuration file (appsettings.json).
        /// </summary>
        const string CONNECTIONSTRINGNAME = "UsersDb";

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersDbFactory"/> class with the specified configuration.
        /// </summary>
        /// <param name="configuration">The application configuration used to retrieve the connection string.</param>
        public UsersDbFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UsersDb"/> context using the configured connection string.
        /// This method is called by EF Core tooling at design time.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        /// <returns>A configured <see cref="UsersDb"/> instance.</returns>
        public UsersDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsersDb>();
            optionsBuilder.UseSqlite(_configuration.GetConnectionString(CONNECTIONSTRINGNAME));
            return new UsersDb(optionsBuilder.Options);
        }
    }
}