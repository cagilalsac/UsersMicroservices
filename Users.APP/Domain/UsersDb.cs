using Microsoft.EntityFrameworkCore;

namespace Users.APP.Domain
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the Users application domain.
    /// Manages the entity sets and provides configuration for database access.
    /// </summary>
    public class UsersDb : DbContext
    {
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> representing groups in the database.
        /// Each <see cref="Group"/> entity corresponds to a record in the Groups table.
        /// </summary>
        public DbSet<Group> Groups { get; set; }



        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="UsersDb"/> class using the specified options.
        /// </summary>
        /// <param name="options">
        /// The options to be used by the <see cref="DbContext"/>, such as the database provider and connection string.
        /// </param>
        public UsersDb(DbContextOptions options) : base(options)
        {
        }
    }
}
