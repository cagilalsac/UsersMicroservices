using Locations.APP.Features.Locations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Locations.API.Controllers
{
    /// <summary>
    /// API controller for handling location-related operations.
    /// Provides endpoints for querying and joining country and city data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        /// <summary>
        /// The mediator instance used to send requests to the appropriate handlers.
        /// </summary>
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationsController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator used for sending requests to handlers.</param>
        public LocationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Executes an inner join query between countries and cities, with support for filtering, ordering, and paging.
        /// The route of the action is api/Locations/InnerJoin.
        /// </summary>
        /// <param name="request">
        /// The <see cref="LocationInnerJoinQueryRequest"/> containing filter, order, and paging parameters.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a list of joined country-city records if found; otherwise, returns NoContent.
        /// </returns>
        /// <remarks>
        /// This endpoint accepts a POST request with a body containing the query parameters.
        /// It delegates the query to the MediatR pipeline, which is handled by <see cref="LocationInnerJoinQueryHandler"/>.
        /// The handler performs the join, applies filters, ordering, and paging, and returns the result as a queryable.
        /// The result is materialized to a list asynchronously. If the list contains any records, an HTTP 200 OK response is returned with the data.
        /// If no records are found, an HTTP 204 No Content response is returned.
        /// </remarks>
        [HttpPost("[action]")]
        public async Task<IActionResult> InnerJoin(LocationInnerJoinQueryRequest request)
        {
            // Send the request to the MediatR pipeline, which will be handled by the appropriate handler.
            var response = await _mediator.Send(request);

            // Materialize the queryable result to a list asynchronously.
            var list = await response.ToListAsync();

            // If the result contains any records, return them with HTTP 200 OK.
            if (list.Any())
                return Ok(list);

            // If no records are found, return HTTP 204 No Content.
            return NoContent();
        }

        /// <summary>
        /// Executes a left join query between countries and cities, with support for filtering, ordering, and paging.
        /// The route of the action is api/Locations/LeftJoin.
        /// </summary>
        /// <param name="request">
        /// The <see cref="LocationLeftJoinQueryRequest"/> containing filter, order, and paging parameters.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a list of joined country-city records if found; otherwise, returns NoContent.
        /// </returns>
        /// <remarks>
        /// This endpoint accepts a POST request with a body containing the query parameters.
        /// It delegates the query to the MediatR pipeline, which is handled by <see cref="LocationLeftJoinQueryHandler"/>.
        /// The handler performs the join, applies filters, ordering, and paging, and returns the result as a queryable.
        /// The result is materialized to a list asynchronously. If the list contains any records, an HTTP 200 OK response is returned with the data.
        /// If no records are found, an HTTP 204 No Content response is returned.
        /// </remarks>
        [HttpPost("[action]")]
        public async Task<IActionResult> LeftJoin(LocationLeftJoinQueryRequest request)
        {
            // Send the request to the MediatR pipeline, which will be handled by the appropriate handler.
            var response = await _mediator.Send(request);

            // Materialize the queryable result to a list asynchronously.
            var list = await response.ToListAsync();

            // If the result contains any records, return them with HTTP 200 OK.
            if (list.Any())
                return Ok(list);

            // If no records are found, return HTTP 204 No Content.
            return NoContent();
        }
    }
}
