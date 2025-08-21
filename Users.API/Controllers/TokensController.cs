using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.APP.Features.Tokens;

namespace Users.API.Controllers
{
    /// <summary>
    /// API controller for handling token-related operations such as generating new JWT and refreshing JWT.
    /// </summary>
    [Route("api/[controller]")] // Sets the base route for this controller to "api/tokens" (controller name is replaced at runtime).
    [ApiController] // Indicates that this class is an API controller and enables automatic model validation and binding.
    public class TokensController : ControllerBase
    {
        private readonly IMediator _mediator; // instance of type implementing IMediator will be injected to this variable in the constructor
        private readonly IConfiguration _configuration; // instance of type implementing IConfiguration will be injected to this variable in the constructor

        /// <summary>
        /// Injects the mediator instance and application's configuration instance to use the previously added sections or sections defined in appsettings.json.
        /// </summary>
        /// <param name="mediator">The mediator instance for sending requests.</param>
        /// <param name="configuration">The application's configuration settings instance for getting configuration values.</param>
        public TokensController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        /// <summary>
        /// Handles HTTP POST requests to generate a new JWT and refresh token for a user.
        /// </summary>
        /// <param name="request">The token request containing user credentials (user name and password).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the token response if authentication operation is successful,
        /// or an error message if authentication operation fails or the request is invalid.
        /// </returns>
        [HttpPost] // Specifies that this action responds to HTTP POST requests.
        [Route("~/api/[action]")] // Overrides the controller's base route. The route becomes "api/Token" (action name is replaced at runtime).
        public async Task<IActionResult> Token(TokenRequest request)
        {
            request.SecurityKey = _configuration["SecurityKey"]; // get the SecurityKey section value from previously added section in Program.cs
            request.Audience = _configuration["Audience"]; // get the Audience section value from appsettings.json
            request.Issuer = _configuration["Issuer"]; // get the Issuer section value from appsettings.json
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);
                if (response is not null)
                    return Ok(response);
                return NotFound(_configuration["TokenMessage:NotFound"]); // return the NotFound section value of the TokenMessage section
                                                                          // from appsettings.json as a HTTP 404 NotFound response
            }
            return BadRequest(_configuration["TokenMessage:BadRequest"]); // return the BadRequest section value of the TokenMessage section
                                                                          // from appsettings.json as a HTTP 400 BadRequest response
        }

        /// <summary>
        /// Handles HTTP POST requests to refresh the JWT and the refresh token for a user.
        /// </summary>
        /// <param name="request">The refresh token request containing the previously generated expired JWT, and refresh token.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the new token response if the refresh operation is successful,
        /// or an error message if the refresh operation fails or the request is invalid.
        /// </returns>
        [HttpPost] // Specifies that this action responds to HTTP POST requests.
        [Route("~/api/[action]")] // Overrides the controller's base route. The route becomes "api/RefreshToken" (action name is replaced at runtime).
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            request.SecurityKey = _configuration["SecurityKey"]; // get the SecurityKey section value from previously added section in Program.cs
            request.Audience = _configuration["Audience"]; // get the Audience section value from appsettings.json
            request.Issuer = _configuration["Issuer"]; // get the Issuer section value from appsettings.json
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);
                if (response is not null)
                    return Ok(response);
                return NotFound(_configuration["TokenMessage:NotFound"]); // return the NotFound section value of the TokenMessage section
                                                                          // from appsettings.json as a HTTP 404 NotFound response
            }
            return BadRequest(_configuration["TokenMessage:BadRequest"]); // return the BadRequest section value of the TokenMessage section
                                                                          // from appsettings.json as a HTTP 400 BadRequest response
        }
    }
}
