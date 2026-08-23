using Microsoft.EntityFrameworkCore;

namespace Users.APP.Domain
{
    // Inherits from the Entity Framework's DbContext class for
    // database configurations and operations.
    public class UsersDb : DbContext
    {
        // Represents the Users table in the database.
        public DbSet<User> Users { get; set; }

        // Represents the Roles table in the database.
        public DbSet<Role> Roles { get; set; }

        // Represents the UserRoles table in the database.
        public DbSet<UserRole> UserRoles { get; set; }

        // Represents the Statuses table in the database.
        public DbSet<Status> Statuses { get; set; }

        // Constructor that takes options of type DbContextOptions
        // parameter and passes it to the base DbContext class constructor.
        // The options parameter is used to configure the database
        // connection with such as the connection string.
        public UsersDb(DbContextOptions options) : base(options)
        {
        }
    }
}
