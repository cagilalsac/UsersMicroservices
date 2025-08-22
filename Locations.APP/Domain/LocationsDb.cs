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
    }
}
