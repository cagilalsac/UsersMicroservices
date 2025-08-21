using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Users.APP.Domain;

namespace Users.API.Controllers
{
    /// <summary>
    /// API controller for database management operations such as seeding initial data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly UsersDb _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseController"/> class.
        /// </summary>
        /// <param name="db">The database context used for data operations.</param>
        public DatabaseController(UsersDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Seeds the database with initial roles, users, and groups.
        /// Removes all existing data from the related tables before inserting new records.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the operation.
        /// Returns HTTP 200 OK with a success message if seeding is successful.
        /// </returns>
        [HttpGet, Route("~/api/SeedDb")]
        public IActionResult Seed()
        {
            // Remove all existing user-role relationships
            var userRoles = _db.UserRoles.ToList();
            _db.UserRoles.RemoveRange(userRoles);

            // Remove all existing roles
            var roles = _db.Roles.ToList();
            _db.Roles.RemoveRange(roles);

            // Remove all existing users
            var users = _db.Users.ToList();
            _db.Users.RemoveRange(users);

            // Remove all existing groups
            var groups = _db.Groups.ToList();
            _db.Groups.RemoveRange(groups);

            // Add default roles
            _db.Roles.Add(new Role()
            {
                Name = "Admin"
            });
            _db.Roles.Add(new Role()
            {
                Name = "User"
            });

            _db.SaveChanges();

            // Add a default group with two users: an admin and a regular user
            _db.Groups.Add(new Group()
            {
                Title = "General",
                Users = new List<User>()
                {
                    new User()
                    {
                        Address = "Çankaya",
                        BirthDate = new DateTime(1980, 8, 21),
                        CityId = 1,
                        CountryId = 1,
                        FirstName = "Çağıl",
                        Gender = Genders.Man,
                        Guid = Guid.NewGuid().ToString(),
                        IsActive = true,
                        LastName = "Alsaç",
                        Password = "admin",
                        RegistrationDate = DateTime.UtcNow,
                        Score = 3.8M,
                        UserName = "admin",
                        UserRoles = new List<UserRole>()
                        {
                            // Assign Admin role to this user
                            new UserRole() { RoleId = _db.Roles.SingleOrDefault(r => r.Name == "Admin").Id }
                        }
                    },
                    new User()
                    {
                        BirthDate = DateTime.Parse("09/13/2004", new CultureInfo("en-US")),
                        CityId = 1,
                        CountryId = 2,
                        FirstName = "Luna",
                        Gender = Genders.Woman,
                        Guid = Guid.NewGuid().ToString(),
                        IsActive = true,
                        LastName = "Leo",
                        Password = "user",
                        RegistrationDate = DateTime.UtcNow,
                        Score = 4.7m,
                        UserName = "user",
                        UserRoles = new List<UserRole>()
                        {
                            // Assign User role to this user
                            new UserRole() { RoleId = _db.Roles.SingleOrDefault(r => r.Name == "User").Id }
                        }
                    },
                }
            });

            _db.SaveChanges();

            return Ok("Database seed successful.");
        }
    }
}