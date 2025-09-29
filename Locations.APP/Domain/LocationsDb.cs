using Microsoft.EntityFrameworkCore;

namespace Locations.APP.Domain
{
    public class LocationsDb : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }

        public LocationsDb(DbContextOptions options) : base(options)
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
            // CountryName data of the Conutries table can not have multiple same values.
            modelBuilder.Entity<Country>().HasIndex(countryEntity => countryEntity.CountryName).IsUnique();

            // CityName data of the Cities table can not have multiple same values.
            modelBuilder.Entity<City>().HasIndex(cityEntity => cityEntity.CityName).IsUnique();



            // Relationship configurations:
            // Configuration should start with the entities that have the foreign keys.
            modelBuilder.Entity<City>()
                .HasOne(cityEntity => cityEntity.Country) // each City entity has one related Country entity
                .WithMany(countryEntity => countryEntity.Cities) // each Country entity has many related City entities
                .HasForeignKey(cityEntity => cityEntity.CountryId) // the foreign key property in the City entity that
                                                                   // references the primary key of the related Country entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of a Country entity if there are related City entities
        }
    }
}
