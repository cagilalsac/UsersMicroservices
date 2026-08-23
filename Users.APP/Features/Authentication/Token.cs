using CORE.Authentication.Models;
using CORE.Authentication.Services;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Authentication
{
    /// <summary>
    /// Represents a token request to obtain a JWT response 
    /// including a JWT (access token) and refresh token.
    /// Inherits from JwtRequest and implements IRequest of 
    /// type JwtResponse for MediatR pipeline integration.
    /// </summary>
    public class TokenRequest : JwtRequest, IRequest<JwtResponse>
    {
        // Inherits user credentials (UserName and Password)
        // with SecurityKey, Issuer and Audience properties
        // from the JwtRequest base class.
    }

    /// <summary>
    /// Handles a TokenRequest by validating user credentials and 
    /// generating a JwtResponse including JWT and refresh token.
    /// </summary>
    public class TokenHandler : DbService<User>,
        IRequestHandler<TokenRequest, JwtResponse>
    {
        // The JWT authentication service that will provide JWT 
        // operations in the methods of this class.
        private readonly IJwtAuthService _jwtAuthService;

        /// <summary>
        /// Initializes a new instance of the TokenHandler class.
        /// </summary>
        /// <param name="db">The injected application's user 
        /// database context through the IoC Container.</param>
        /// <param name="jwtAuthService">The injected JWT 
        /// authentication service instance through the 
        /// IoC Container.</param>
        public TokenHandler(DbContext db,
            IJwtAuthService jwtAuthService) : base(db)
        {
            _jwtAuthService = jwtAuthService;
        }

        /// <summary>
        /// Returns a queryable collection of User entities with 
        /// their associated UserRole and Role navigation properties 
        /// eagerly included.
        /// Overrides the base method to apply eager loading.
        /// </summary>
        /// <returns>
        /// An IQueryable of type User with the UserRole and Role 
        /// navigation properties eagerly loaded.
        /// </returns>
        protected override IQueryable<User> DbQuery()
        {
            return base.DbQuery()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);
            // u: User entity delegate, ur: UserRole entity delegate
        }

        /// <summary>
        /// Handles the token request by authenticating the user and 
        /// returning a JWT response including JWT and refresh token.
        /// </summary>
        /// <param name="request">The token request containing username, 
        /// password, security key, audience and issuer.</param>
        /// <param name="cancellationToken">Asynchronous method's token 
        /// to cancel the operation.</param>
        /// <returns>A JwtResponse including the JWT and refresh token 
        /// if successful, otherwise null.</returns>
        public async Task<JwtResponse> Handle(TokenRequest request, 
            CancellationToken cancellationToken)
        {
            // Attempt to get the active user by user name and password
            var userEntity = await DbSingleAsync(
                u => u.UserName == request.UserName && 
                u.Password == request.Password && u.StatusId == 1, 
                cancellationToken);
                // "Active" status ID value is 1 in the Statuses table.
                // Instead of u.StatusId == 1, u.Status.Title == "Active"
                // may also be written. 
            // u: User entity delegate, ur: UserRole entity delegate.

            // If user entity is not found, return null
            if (userEntity is null)
                return null;

            // Generate refresh token and save it to the Users table
            // for the retrieved user entity with expiration date and time,
            // Also update the user entity's online status
            userEntity.RefreshToken = _jwtAuthService.GetRefreshToken();
            userEntity.RefreshTokenExpiration = DateTime.Now.AddDays(7);
            userEntity.IsOnline = true;
            // the refresh token will expire after 7 days from
            // DateTime.Now's execution value
            await DbUpdateAsync(userEntity, cancellationToken);

            // Return a JWT response according to the expiration
            // including the JWT and refresh token
            var expiration = DateTime.Now.AddMinutes(5); 
            // the JWT will expire after 5 minutes from DateTime.Now's
            // execution value
            return _jwtAuthService.GetJwtResponse(userEntity.Id, 
                userEntity.UserName, 
                userEntity.UserRoles.Select(ur => ur.Role.Name),
                expiration, request.SecurityKey, request.Issuer, 
                request.Audience, userEntity.RefreshToken);
        }
    }
}
