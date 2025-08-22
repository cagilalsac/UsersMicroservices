using Locations.APP.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Locations.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly LocationsDb _db;
        private readonly IWebHostEnvironment _environment;

        public DatabaseController(LocationsDb db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpGet, Route("~/api/SeedDb")]
        public IActionResult Seed()
        {
            if (!_environment.IsDevelopment())
                return BadRequest("The seed operation can only be performed in development environment!");

            _db.Cities.RemoveRange(_db.Cities.ToList());
            _db.Countries.RemoveRange(_db.Countries.ToList());

            _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Cities';");
            _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Countries';");

            _db.Countries.Add(new Country
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Türkiye",
                Cities = new List<City>
                {
                    new City
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "Ankara"
                    },
                    new City
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "İstanbul"
                    },
                    new City
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "İzmir"
                    }
                }
            });
            _db.Countries.Add(new Country
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "United States of America",
                Cities = new List<City>
                {
                    new City
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "New York"
                    },
                    new City
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "Los Angeles"
                    }
                }
            });

            _db.SaveChanges();

            return Ok("Database seed in the development environment successful.");
        }
    }
}
