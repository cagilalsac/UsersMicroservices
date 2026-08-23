using Microsoft.AspNetCore.Mvc;
using Users.APP.Domain;

namespace Users.API.Controllers
{
    // Database Controller for seeding the database with initial data
    // through the Seed action.
    [Route("api/[controller]")] // route for the controller: api/UsersDb
    [ApiController] // attribute indicates that this is an API controller
    public class UsersDbController : ControllerBase
    {
        [HttpGet, Route("~/api/SeedDb")] // route for the get action
                                         // changed with the Route
                                         // attribute: api/SeedDb
        public IActionResult Seed()
        {
            // Initialize the UsersDbFactory instance.
            var dbFactory = new UsersDbFactory();

            // Seed the database using the SeedDb method.
            dbFactory.SeedDb();

            // Return HTTP OK status result (200) with a success message.
            return Ok("Database seed successful.");
        }
    }
}
