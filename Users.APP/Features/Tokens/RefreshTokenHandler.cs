using CORE.APP.Models.Authentication;
using CORE.APP.Services;
using CORE.APP.Services.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Tokens
{
    /// <summary>
    /// Represents the request for returning a token response including JWT (access token) and refresh token.
    /// Inherits from the RefreshTokenRequestBase class to have token, refresh token and security key in the request
    /// and implements IRequest<TokenResponse> for MediatR pipeline integration.
    /// </summary>
    public class RefreshTokenRequest : RefreshTokenRequestBase, IRequest<TokenResponse>
    {
    }

    /// <summary>
    /// Handles the logic for processing a refresh token request to return a token response.
    /// Validates if the refresh token is expired or not through a User entity query and returns a new token response 
    /// including the new JWT (access token) and new refresh token if valid, otherwise returns null.
    /// </summary>
    public class RefreshTokenHandler : Service<User>, IRequestHandler<RefreshTokenRequest, TokenResponse>
    {
        /// <summary>
        /// The token authentication service that will provide token operations in the methods of this class.
        /// </summary>
        private readonly ITokenAuthService _tokenAuthService; // readonly means the value or instance can only be assigned
                                                              // at this line or through the constructor

        /// <summary>
        /// Initializes a new instance of this class with the given database context and token authentication service.
        /// </summary>
        /// <param name="db">The UsersDb database context.</param>
        /// <param name="tokenAuthService">The injected token authentication service instance through the IoC Container.</param>
        public RefreshTokenHandler(DbContext db, ITokenAuthService tokenAuthService) : base(db)
        {
            _tokenAuthService = tokenAuthService;
        }

        /// <summary>
        /// Overridden method of the base class for eager loading of UserRole and Role entities.
        /// </summary>
        /// <param name="isNoTracking">If true, changes of the entities in the DbSet will not be tracked by Entity Framework, 
        /// otherwise changes will be tracked.</param>
        /// <returns>The eagerly loaded User entity query with UserRoles and Role.</returns>
        protected override IQueryable<User> Query(bool isNoTracking = true)
        {
            // u: User entity delegate, ur: UserRole entity delegate
            return base.Query(isNoTracking).Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
        }

        /// <summary>
        /// Handles the refresh token logic: verifies the user with refresh token expiration, then returns a new token response
        /// including the JWT (access token) and refresh token if verified. Otherwise returns null.
        /// </summary>
        /// <param name="request">The refresh token request object containing the JWT (access token), refresh token and security key.</param>
        /// <param name="cancellationToken">Asynchronous method's token to cancel the operation.</param>
        /// <returns>A token response containing the result of the operation.</returns>
        public async Task<TokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            // Extract the user's claims from request's expired token and security key
            var claims = _tokenAuthService.GetClaims(request.Token, request.SecurityKey);

            // Extract the user ID from claims
            var userId = Convert.ToInt32(claims.SingleOrDefault(claim => claim.Type == "Id").Value);

            // Find user in the Users Table that matches the ID and has a non expired refresh token
            // isNoTracking is false for being tracked by EF Core to update the entity
            var user = await Query(false).SingleOrDefaultAsync(user => user.Id == userId && user.RefreshToken == request.RefreshToken 
                && user.RefreshTokenExpiration >= DateTime.Now, cancellationToken);

            // If user is not found, return null
            if (user is null)
                return null;

            // Generate a new refresh token (for added security)
            user.RefreshToken = _tokenAuthService.GetRefreshToken();

            // Optional: Enable sliding expiration for the refresh token
            // user.RefreshTokenExpiration = DateTime.Now.AddDays(7);

            // Save the updated user state to the database
            await Update(user, cancellationToken);

            // return a token response according to the expiration including the JWT and refresh token.
            var expiration = DateTime.Now.AddMinutes(5);
            return _tokenAuthService.GetTokenResponse(user.Id, user.UserName, user.UserRoles.Select(ur => ur.Role.Name).ToArray(), 
                expiration, request.SecurityKey, request.Issuer, request.Audience, user.RefreshToken);
        }
    }
}
