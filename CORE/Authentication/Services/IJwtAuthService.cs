using CORE.Authentication.Models;
using System.Security.Claims;

namespace CORE.Authentication.Services
{
    /// <summary>
    /// Provides JWT (JSON Web Token) based authentication operations, 
    /// including access token (JWT) and refresh token generation and 
    /// claim extraction.
    /// </summary>
    public interface IJwtAuthService
    {
        /// <summary>
        /// Returns a JWT response including JWT (access token) and 
        /// refresh token.
        /// </summary>
        /// <param name="userId">The unique ID of the user.</param>
        /// <param name="userName">The username of the user.</param>
        /// <param name="userRoleNames">A collection of role names 
        /// assigned to the user.</param>
        /// <param name="expiration">The expiration date and time for 
        /// the JWT.</param>
        /// <param name="securityKey">The security key used to sign 
        /// the JWT.</param>
        /// <param name="issuer">The issuer of the JWT, generally the 
        /// server API application's domain.</param>
        /// <param name="audience">The intended audience for the JWT, 
        /// generally the client application's domain.</param>
        /// <param name="refreshToken">Refresh token to be included 
        /// in the returned JwtResponse object.</param>
        /// <returns>A JwtResponse object containing the created JWT 
        /// and provided refresh token.</returns>
        public JwtResponse GetJwtResponse(int userId, string userName, 
            IEnumerable<string> userRoleNames, DateTime expiration,
            string securityKey, string issuer, 
            string audience, string refreshToken);
        // public may not be written

        /// <summary>
        /// Returns a new generated refresh token, which is a secure, 
        /// random string used to obtain new JWT without 
        /// re-authenticating the user.
        /// </summary>
        /// <returns>
        /// A string representing the newly generated refresh token.
        /// </returns>
        public string GetRefreshToken();

        /// <summary>
        /// Extracts and returns a collection of claims from the 
        /// specified access token (JWT) using the provided security key.
        /// </summary>
        /// <param name="jwt">The JWT containing encoded claims.</param>
        /// <param name="securityKey">The security key used to validate 
        /// and decode the JWT.</param>
        /// <returns>
        /// A claim collection containing the claims extracted from the JWT.
        /// </returns>
        public IEnumerable<Claim> GetClaims(string jwt, string securityKey);
    }
}
