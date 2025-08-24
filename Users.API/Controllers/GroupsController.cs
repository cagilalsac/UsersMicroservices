using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.APP.Features.Groups;

namespace Users.API.Controllers
{
    /// <summary>
    /// API controller for managing group-related operations.
    /// Handles HTTP requests for inserting, updating, deleting or retrieving group data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // Way 1:
    //[Authorize(Roles = "Admin,User")] // Only authenticated users with Admin or User role can execute all of the actions of this controller.
    // Way 2:
    [Authorize] // Only authenticated users can execute all of the actions of this controller.
                // Since we have only 2 roles Admin and User, we can use Authorize to check auhenticated users without defining roles.
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsController"/> class.
        /// </summary>
        /// <param name="mediator">
        /// The <see cref="IMediator"/> instance used to send requests to MediatR handlers.
        /// Enables decoupled request/response processing for queries and commands.
        /// </param>
        public GroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Handles HTTP GET requests to retrieve all groups as <see cref="GroupQueryResponse"/> list.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a list of group query response data.
        /// Returns HTTP 200 OK with the list of groups on success.
        /// </returns>
        /// <remarks>
        /// This action uses MediatR to send a <see cref="GroupQueryRequest"/> to its handler (<see cref="GroupQueryHandler"/>).
        /// The handler returns an <see cref="IQueryable{GroupQueryResponse}"/>, which is then
        /// asynchronously materialized into a list using Entity Framework Core's <c>ToListAsync</c>.
        /// The result is returned as a JSON array in the HTTP response body.
        /// </remarks>
        [HttpGet] // HttpGet: attribute also called action method, get route: /Groups
        [AllowAnonymous] // Both authenticated and unauthenticated users (everyone) can execute this action.
                         // Overrides the Authorize defined for the controller.
        public async Task<IActionResult> Get()
        {
            // Send a GroupQueryRequest to MediatR, which dispatches it to the appropriate handler (GroupQueryHandler).
            var query = await _mediator.Send(new GroupQueryRequest());

            // Execute the query and retrieve the results as a list asynchronously.
            var list = await query.ToListAsync();

            // Return the list of groups with HTTP 200 OK.
            return Ok(list);

            /*
            ActionResult inheritance:  
            IActionResult: general return type of actions in a controller  
            |  
            ActionResult: base class that implements IActionResult  
            |  
            OkObjectResult (returned by Ok method) - NotFoundResult (returned by NotFound method) - 
            BadRequestObjectResult (returned by BadRequest method) - etc.
            */
        }

        /// <summary>
        /// Handles HTTP GET requests to retrieve a single group by its unique identifier.
        /// </summary>
        /// <param name="id">The integer unique identifier of the group item to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing an item of group query response data if found.
        /// Returns HTTP 200 OK with the group data if the group exists, or HTTP 404 Not Found if no group matches the specified ID.
        /// </returns>
        /// <remarks>
        /// This action uses MediatR to send a <see cref="GroupQueryRequest"/> to its handler (<see cref="GroupQueryHandler"/>).
        /// The handler returns an <see cref="IQueryable{GroupQueryResponse}"/> representing all groups.
        /// The method then filters this query to find a group with the specified <paramref name="id"/> 
        /// using Entity Framework Core's <c>SingleOrDefaultAsync</c>.
        /// If a matching group is found, it is returned as a JSON object with HTTP 200 OK.
        /// If no group is found, HTTP 404 Not Found is returned.
        /// </remarks>
        [HttpGet("{id}")] // get route: /Groups/5 (name defined in {} must be same as the action's parameter name, id will be 5)
        // Only authenticated users (since Authorize is defined at the controller level) can execute this action.
        public async Task<IActionResult> Get(int id)
        {
            // Send a GroupQueryRequest to MediatR, which dispatches it to the appropriate handler (GroupQueryHandler).
            // The handler returns an IQueryable<GroupQueryResponse> representing all groups.
            var query = await _mediator.Send(new GroupQueryRequest());

            // Asynchronously find the group with the specified ID.
            // If no group matches, item will be null.
            var item = await query.SingleOrDefaultAsync(groupResponse => groupResponse.Id == id);

            // If the group is not found, return HTTP 404 Not Found.
            if (item is null)
                return NotFound();

            // If the group is found, return it with HTTP 200 OK.
            return Ok(item);
        }

        /// <summary>
        /// Handles HTTP POST requests to create a new group.
        /// </summary>
        /// <param name="request">The <see cref="GroupCreateRequest"/> containing the Title data for the new group.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the create operation.
        /// - Returns HTTP 201 Created Status Code with the location of the new group if successful.
        /// - Returns HTTP 400 Bad Request Status Code with validation or business error details if unsuccessful.
        /// </returns>
        /// <remarks>
        /// This action receives a group creation request from the client and first checks if the model state is valid
        /// (i.e., all data annotations are satisfied). If valid, it sends the request to the MediatR pipeline,
        /// which dispatches it to the appropriate handler (<see cref="GroupCreateHandler"/>).
        /// The handler processes the creation logic and returns a <see cref="CORE.APP.Models.CommandResponse"/> indicating success or failure.
        /// - If the operation is successful, the method returns HTTP 201 Created, using <c>CreatedAtAction</c> to provide the new group.
        /// - If the operation fails (e.g., duplicate group title), it returns HTTP 400 Bad Request with the error message.
        /// - If the model state is invalid, it returns HTTP 400 Bad Request with validation errors.
        /// </remarks>
        [HttpPost] // post route: /Groups
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
                                     // Overrides the Authorize defined for the controller.
        public async Task<IActionResult> Post(GroupCreateRequest request)
        {
            // Check if the incoming request model passes validations through data annotations.
            if (ModelState.IsValid)
            {
                // Send the creation request to MediatR, which will route it to the appropriate handler (GroupCreateHandler).
                var response = await _mediator.Send(request);

                // If the group was created successfully
                if (response.IsSuccessful)
                {
                    // Way 1: return HTTP 200 OK with the success command response.
                    //return Ok(response);
                    // Way 2: return HTTP 201 Created with the location of the new group and the success command response.
                    return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
                }

                // If the creation failed due to business logic (e.g., duplicate title), return HTTP 400 Bad Request with the command response.
                return BadRequest(response);
            }

            // If the model state is invalid, return HTTP 400 Bad Request with validation error details.
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Handles HTTP PUT requests to update an existing group.
        /// </summary>
        /// <param name="request">The <see cref="GroupUpdateRequest"/> containing the group ID and the new title.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the update operation.
        /// - Returns HTTP 204 No Content Status Code if the update is successful.
        /// - Returns HTTP 400 Bad Request Status Code with validation or business error details if unsuccessful.
        /// </returns>
        /// <remarks>
        /// This action receives a group update request from the client and first checks if the model state is valid
        /// (i.e., all data annotations are satisfied). If valid, it sends the request to the MediatR pipeline,
        /// which dispatches it to the appropriate handler (<see cref="GroupUpdateHandler"/>).
        /// The handler processes the update logic and returns a <see cref="CORE.APP.Models.CommandResponse"/> indicating success or failure.
        /// - If the operation is successful, the method returns HTTP 204 No Content, indicating the update was successful but no content is returned.
        /// - If the operation fails (e.g., duplicate group title or group not found), it returns HTTP 400 Bad Request with the command response.
        /// - If the model state is invalid, it returns HTTP 400 Bad Request with validation errors.
        /// </remarks>
        [HttpPut] // put route: /Groups
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
                                     // Overrides the Authorize defined for the controller.
        public async Task<IActionResult> Put(GroupUpdateRequest request)
        {
            // Check if the incoming request model passes validations through data annotations.
            if (ModelState.IsValid)
            {
                // Send the update request to MediatR, which will route it to the appropriate handler (GroupUpdateHandler).
                var response = await _mediator.Send(request);

                // If the group was updated successfully
                if (response.IsSuccessful)
                {
                    // Way 1: return HTTP 200 OK with the success command response.
                    //return Ok(response);
                    // Way 2: return HTTP 204 No Content (no response body).
                    return NoContent();
                }

                // If the update failed due to business logic (e.g., duplicate title or group not found), return HTTP 400 Bad Request with the command response.
                return BadRequest(response);
            }

            // If the model state is invalid, return HTTP 400 Bad Request with validation error details.
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Handles HTTP DELETE requests to remove a group by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// The integer unique identifier of the group to delete. This value is provided in the route (e.g., /Groups/5).
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the delete operation:
        /// - Returns HTTP 204 No Content Status Code if the group was deleted successfully.
        /// - Returns HTTP 400 Bad Request Status Code with error details if the deletion failed (e.g., group not found, relational records found).
        /// </returns>
        /// <remarks>
        /// This action uses MediatR to send a <see cref="GroupDeleteRequest"/> to its handler.
        /// The handler performs the deletion logic and returns a <see cref="CORE.APP.Models.CommandResponse"/> indicating success or failure.
        /// - If the operation is successful, the method returns HTTP 204 No Content, indicating the group was deleted.
        /// - If the operation fails (e.g., group does not exist, relational records found), it returns HTTP 400 Bad Request with error command response.
        /// </remarks>
        [HttpDelete("{id}")] // delete route: /Groups/5 (name defined in {} must be same as the action's parameter name, id will be 5)
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
                                     // Overrides the Authorize defined for the controller.
        public async Task<IActionResult> Delete(int id)
        {
            // Create a GroupDeleteRequest with the specified group ID and send it to MediatR.
            // MediatR dispatches the request to the appropriate handler (GroupDeleteHandler).
            var response = await _mediator.Send(new GroupDeleteRequest() { Id = id });

            // If group was deleted successfully
            if (response.IsSuccessful)
            {
                // Way 1: return HTTP 200 OK with the success command response.
                // return Ok(response); 
                // Way 2: return HTTP 204 No Content (no response body).
                return NoContent(); 
            }

            // If the deletion failed (e.g., group not found, relational records found), return HTTP 400 Bad Request with error details.
            return BadRequest(response);
        }
    }
}
