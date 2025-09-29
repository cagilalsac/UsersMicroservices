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



        // Overriding OnModelCreating method is optional.
        /// <summary>
        /// Configures the entity relationships and database schema rules for the application domain.
        /// This method defines how entities are related, sets up foreign key constraints, and customizes
        /// the delete behavior for each relationship to prevent cascading deletes.
        /// </summary>
        /// <param name="modelBuilder">
        /// The <see cref="ModelBuilder"/> used to configure entity mappings and relationships.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Index configurations:
            // Defining unique indices to enforce uniqueness constraints on certain properties.
            // Title data of the Groups table can not have multiple same values.
            modelBuilder.Entity<Group>().HasIndex(groupEntity => groupEntity.Title).IsUnique();

            // Name data of the Roles table can not have multiple same values.
            modelBuilder.Entity<Role>().HasIndex(roleEntity => roleEntity.Name).IsUnique();

            // UserName data of the Users table can not have multiple same values.
            modelBuilder.Entity<User>().HasIndex(userEntity => userEntity.UserName).IsUnique();

            // Defining indices for optimizing query performance on frequently searched properties.
            modelBuilder.Entity<User>().HasIndex(userEntity => userEntity.CountryId);
            modelBuilder.Entity<User>().HasIndex(userEntity => userEntity.CityId);

            // Composite index on FirstName and LastName for optimizing searches involving both fields.
            modelBuilder.Entity<User>().HasIndex(userEntity => new { userEntity.FirstName, userEntity.LastName });



            // Relationship configurations:
            // Configuration should start with the entities that have the foreign keys.
            modelBuilder.Entity<UserRole>()
                .HasOne(userRoleEntity => userRoleEntity.User) // each UserRole entity has one related User entity
                .WithMany(userEntity => userEntity.UserRoles) // each User entity has many related UserRole entities
                .HasForeignKey(userRoleEntity => userRoleEntity.UserId) // the foreign key property in the UserRole entity that
                                                                        // references the primary key of the related User entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of a User entity if there are related UserRole entities

            modelBuilder.Entity<UserRole>()
                .HasOne(userRoleEntity => userRoleEntity.Role) // each UserRole entity has one related Role entity
                .WithMany(roleEntity => roleEntity.UserRoles) // each Role entity has many related UserRole entities
                .HasForeignKey(userRoleEntity => userRoleEntity.RoleId) // the foreign key property in the UserRole entity that
                                                                        // references the primary key of the related Role entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of a Role entity if there are related UserRole entities

            modelBuilder.Entity<User>()
                .HasOne(userEntity => userEntity.Group) // each User entity has one related Group entity
                .WithMany(groupEntity => groupEntity.Users) // each Group entity has many related User entities
                .HasForeignKey(userEntity => userEntity.GroupId) // the foreign key property in the User entity that
                                                                 // references the primary key of the related Group entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of a Group entity if there are related User entities
        }
    }
}
