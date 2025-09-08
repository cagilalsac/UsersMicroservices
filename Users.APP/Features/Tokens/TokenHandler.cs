using CORE.APP.Models.Authentication;
using CORE.APP.Services;
using CORE.APP.Services.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Tokens
{
    /// <summary>
    /// Represents a token request to obtain a token response, including a JWT (access token) and refresh token.
    /// Inherits from <see cref="TokenRequestBase"/> and implements <see cref="IRequest{TokenResponse}"/> for MediatR pipeline integration.
    /// </summary>
    public class TokenRequest : TokenRequestBase, IRequest<TokenResponse>
    {
    }

    /// <summary>
    /// Handles a <see cref="TokenRequest"/> by validating credentials and generating a <see cref="TokenResponse"/> including JWT and refresh token.
    /// </summary>
    public class TokenHandler : Service<User>, IRequestHandler<TokenRequest, TokenResponse>
    {
        /// <summary>
        /// The token authentication service that will provide token operations in the methods of this class.
        /// </summary>
        private readonly ITokenAuthService _tokenAuthService; // readonly means the value or instance can only be assigned
                                                              // at this line or through the constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHandler"/> class.
        /// </summary>
        /// <param name="db">The application's user database context.</param>
        /// <param name="tokenAuthService">The injected token authentication service instance through the IoC Container.</param>
        public TokenHandler(DbContext db, ITokenAuthService tokenAuthService) : base(db)
        {
            _tokenAuthService = tokenAuthService;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="User"/> entities with their associated 
        /// <see cref="UserRole"/> and <see cref="Role"/> navigation properties eagerly included.
        /// Overrides the base method to apply eager loading.
        /// </summary>
        /// <param name="isNoTracking">
        /// If <c>true</c>, disables change tracking for better performance in read-only operations.
        /// If <c>false</c>, enables change tracking to allow updates to the queried entities.
        /// </param>
        /// <returns>
        /// An <see cref="IQueryable{User}"/> with the <see cref="UserRole"/> and <see cref="Role"/> navigation properties eagerly loaded.
        /// </returns>
        protected override IQueryable<User> Query(bool isNoTracking = true)
        {
            // u: User entity delegate, ur: UserRole entity delegate
            return base.Query(isNoTracking).Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
        }

        /// <summary>
        /// Handles the token request by authenticating the user and returning a token response including JWT and refresh token.
        /// </summary>
        /// <param name="request">The token request containing username, password, security key, audience and issuer.</param>
        /// <param name="cancellationToken">Asynchronous method's token to cancel the operation.</param>
        /// <returns>A <see cref="TokenResponse"/> including the JWT and refresh token if successful, otherwise null.</returns>
        public async Task<TokenResponse> Handle(TokenRequest request, CancellationToken cancellationToken)
        {
            // Attempt to retrieve the active user by user name and password
            // isNoTracking is false for being tracked by EF Core to update the entity
            var user = await Query(false).SingleOrDefaultAsync(u => u.UserName == request.UserName && u.Password == request.Password 
                && u.IsActive, cancellationToken);

            // If user not found, return null
            if (user is null)
                return null;

            // Generate refresh token and save it to the Users table for the retrieved user with expiration date and time
            user.RefreshToken = _tokenAuthService.GetRefreshToken();
            user.RefreshTokenExpiration = DateTime.Now.AddDays(7); // the refresh token will expire after 7 days from DateTime.Now's execution value
            await Update(user, cancellationToken);

            // return a token response according to the expiration including the JWT and refresh token.
            var expiration = DateTime.Now.AddMinutes(5); // the JWT will expire after 5 minutes from DateTime.Now's execution value
            return _tokenAuthService.GetTokenResponse(user.Id, user.UserName, user.UserRoles.Select(ur => ur.Role.Name).ToArray(), 
                expiration, request.SecurityKey, request.Issuer, request.Audience, user.RefreshToken);
        }
    }
}
