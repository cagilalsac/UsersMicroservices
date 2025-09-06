#nullable disable
using CORE.APP.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.APP.Features.Users;

//Generated from Custom Microservices Template.
namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IMediator _mediator;

        // Constructor: injects logger to log the errors to Kestrel Console or Output Window and mediator
        public UsersController(ILogger<UsersController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // GET: api/Users
        [HttpGet]
        // Way 1:
        //[Authorize(Roles = "Admin,User")] // Only authenticated users with role Admin or User can execute this action.
        // Way 2: since we have only 2 roles Admin and User, we can use Authorize to check auhenticated users without defining roles.
        [Authorize]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Send a query request to get query response
                var response = await _mediator.Send(new UserQueryRequest());
                // Convert the query response to a list
                var list = await response.ToListAsync();
                // If there are items, return them with 200 OK
                if (list.Any())
                    return Ok(list);
                // If no items found, return 204 No Content
                return NoContent();
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("UsersGet Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during UsersGet."));
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Send a query request to get query response
                var response = await _mediator.Send(new UserQueryRequest());
                // Find the item with the given id
                var item = await response.SingleOrDefaultAsync(r => r.Id == id);
                // If item found, return it with 200 OK
                if (item is not null)
                    return Ok(item);
                // If item not found, return 204 No Content
                return NoContent();
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("UsersGetById Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during UsersGetById."));
            }
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Post(UserCreateRequest request)
        {
            try
            {
                // Check if the request model is valid through data annotations
                if (ModelState.IsValid)
                {
                    // Send the create request
                    var response = await _mediator.Send(request);
                    // If creation is successful, return 200 OK with success command response
                    if (response.IsSuccessful)
                    {
                        //return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
                        return Ok(response);
                    }
                    // If creation failed, add error command response message to model state
                    ModelState.AddModelError("UsersPost", response.Message);
                }
                // Return 400 Bad Request with all data annotation validation error messages and the error command response message if added seperated by |
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("UsersPost Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during UsersPost."));
            }
        }

        // PUT: api/Users
        [HttpPut]
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Put(UserUpdateRequest request)
        {
            try
            {
                // Check if the request model is valid through data annotations
                if (ModelState.IsValid)
                {
                    // Send the update request
                    var response = await _mediator.Send(request);
                    // If update is successful, return 200 OK with success command response
                    if (response.IsSuccessful)
                    {
                        //return NoContent();
                        return Ok(response);
                    }
                    // If update failed, add error command response message to model state
                    ModelState.AddModelError("UsersPut", response.Message);
                }
                // Return 400 Bad Request with all data annotation validation error messages and the error command response message if added seperated by |
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("UsersPut Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during UsersPut."));
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Send the delete request
                var response = await _mediator.Send(new UserDeleteRequest() { Id = id });
                // If delete is successful, return 200 OK with success command response
                if (response.IsSuccessful)
                {
                    //return NoContent();
                    return Ok(response);
                }
                // If delete failed, add error command response message to model state
                ModelState.AddModelError("UsersDelete", response.Message);
                // Return 400 Bad Request with the error command response message
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("UsersDelete Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during UsersDelete."));
            }
        }



        /// <summary>
        /// Returns a response with filtered Users data according to the request.
        /// The post request can be sent to the route api/Users/GetFiltered.
        /// </summary>
        /// <param name="request">A user query request to filter Users data.</param>
        /// <returns>A success response with filtered user query response data if any, otherwise no content response.</returns>
        [HttpPost("[action]")]
        // Way 1:
        //[Authorize(Roles = "Admin,User")] // Only authenticated users with role Admin or User can execute this action.
        // Way 2: since we have only 2 roles Admin and User, we can use Authorize to check auhenticated users without defining roles.
        [Authorize]
        public async Task<IActionResult> GetFiltered(UserQueryRequest request)
        {
            var response = await _mediator.Send(request);
            var list = await response.ToListAsync();
            if (list.Any())
                return Ok(list);
            return NoContent();
        }



        /// <summary>
        /// Retrieves a list of user location data, including countries and cities, by calling external APIs.
        /// The GET request can be sent to the route api/Users/GetUserLocations.
        /// </summary>
        /// <remarks>
        /// This action sends a <see cref="UserLocationQueryRequest"/> to the mediator, which fetches location data
        /// from the specified Countries and Cities API endpoints. The response is returned as an HTTP 200 OK result
        /// containing the list of locations.
        /// </remarks>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the list of user locations if successful.
        /// </returns>
        [HttpGet("[action]")]
        [Authorize] // Only authenticated users can execute this action.
        public async Task<IActionResult> GetUserLocations()
        {
            var list = await _mediator.Send(new UserLocationQueryRequest
            {
                // Gateway Countries API URL
                CountriesApiUrl = "https://localhost:7237/api/countries",

                // Gateway Cities API URL
                CitiesApiUrl = "https://localhost:7237/api/cities"
            });
            return Ok(list);
        }
    }
}
