using CORE.Authentication.Models;
using CORE.Authentication.Services;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Authentication
{
    /// <summary>
    /// Represents the request for returning a JWT response 
    /// including JWT and refresh token.
    /// Inherits from the JwtRefreshRequest base class to have 
    /// Jwt, RefreshToken, SecurityKey, Issuer and Audience 
    /// properties, and implements IRequest of type JwtResponse 
    /// for MediatR pipeline integration.
    /// </summary>
    public class RefreshTokenRequest : JwtRefreshRequest, 
        IRequest<JwtResponse>
    {
    }

    /// <summary>
    /// Handles the logic for processing a refresh token request 
    /// to return a JWT response.
    /// Validates if the refresh token is expired or not through 
    /// a User entity query and returns a new JWT response including 
    /// the new JWT and new refresh token if valid, 
    /// otherwise returns null.
    /// </summary>
    public class RefreshTokenHandler : DbService<User>,
        IRequestHandler<RefreshTokenRequest, JwtResponse>
    {
        // The JWT authentication service that will provide JWT 
        // operations in the methods of this class.
        private readonly IJwtAuthService _jwtAuthService;

        /// <summary>
        /// Initializes a new instance of the RefreshTokenHandler 
        /// class.
        /// </summary>
        /// <param name="db">The injected application's user 
        /// database context through the IoC Container.</param>
        /// <param name="jwtAuthService">The injected JWT 
        /// authentication service instance through the 
        /// IoC Container.</param>

        public RefreshTokenHandler(DbContext db, 
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
        /// Handles the refresh token logic: verifies the user with 
        /// refresh token expiration, then returns a new JWT response
        /// including the JWT and refresh token if verified. 
        /// Otherwise returns null.
        /// </summary>
        /// <param name="request">The refresh token request object 
        /// containing the JWT, refresh token, security key, issuer 
        /// and audience.</param>
        /// <param name="cancellationToken">Asynchronous method's token 
        /// to cancel the operation.</param>
        /// <returns>A JWT response containing the result of the 
        /// operation.</returns>
        public async Task<JwtResponse> Handle(RefreshTokenRequest request, 
            CancellationToken cancellationToken)
        {
            // Extract the user's claims from request's expired JWT
            // and security key
            var claims = _jwtAuthService.GetClaims(request.Jwt, 
                request.SecurityKey);

            // Extract the user ID from claims
            var userId = Convert.ToInt32(claims
                .SingleOrDefault(claim => claim.Type == "Id").Value);

            // Find user entity in the Users database table that matches
            // the ID and has a non expired refresh token
            var userEntity = await DbSingleAsync(user => 
                user.Id == userId && 
                user.RefreshToken == request.RefreshToken &&
                user.RefreshTokenExpiration >= DateTime.Now, 
                cancellationToken);

            // If user entity is not found, return null
            if (userEntity is null)
                return null;

            // Generate a new refresh token (for added security)
            userEntity.RefreshToken = _jwtAuthService.GetRefreshToken();

            // Optional: Enable sliding expiration for the refresh token
            // userEntity.RefreshTokenExpiration = DateTime.Now.AddDays(7);

            // Save the updated user entity state to the database
            await DbUpdateAsync(userEntity, cancellationToken);

            // Return a JWT response according to the expiration including
            // the new JWT and new refresh token
            var expiration = DateTime.Now.AddMinutes(5);
            return _jwtAuthService.GetJwtResponse(userEntity.Id, 
                userEntity.UserName, 
                userEntity.UserRoles.Select(ur => ur.Role.Name),
                expiration, request.SecurityKey, request.Issuer, 
                request.Audience, userEntity.RefreshToken);
        }
    }
}
