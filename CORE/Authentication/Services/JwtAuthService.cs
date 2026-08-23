using CORE.Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CORE.Authentication.Services
{
    /// <summary>
    /// Provides concrete implementations for JWT (JSON Web Token) 
    /// based authentication operations, including generating JWT 
    /// response with JWT (access token) and refresh token,
    /// generating refresh token, and extracting claims from JWT.
    /// This service is responsible for securely creating and 
    /// validating JWT used in authentication flows.
    /// </summary>
    public class JwtAuthService : IJwtAuthService
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
            string audience, string refreshToken)
        {
            // Create claims for user ID and username,
            // then add claims for each user role.
            var claims = new List<Claim>
            {
                new Claim("Id", userId.ToString()), 
                // custom claim with key Id and value user ID
                new Claim(ClaimTypes.Name, userName)
            };
            foreach (var userRoleName in userRoleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRoleName));
            }

            // Create signing credentials using the provided security key
            // and 256-bit hash.
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(securityKey));
            var signingCredentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);

            // Build the JWT with claims, issuer, audience, and expiration.
            var jwtSecurityToken = new JwtSecurityToken(
                issuer, audience, claims, DateTime.Now, expiration, 
                signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            // Serialize the JWT to a string.
            var jwt = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

            // Return the JWT response with the serialized JWT value
            // and the refresh token parameter value.
            return new JwtResponse
            {
                Jwt = $"{JwtBearerDefaults.AuthenticationScheme} {jwt}", 
                // JwtBearerDefaults.AuthenticationScheme: "Bearer"
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Returns a new generated refresh token, which is a secure, 
        /// random string used to obtain new JWT without 
        /// re-authenticating the user.
        /// </summary>
        /// <returns>
        /// A string representing the newly generated refresh token.
        /// </returns>
        public string GetRefreshToken()
        {
            // Generate a cryptographically secure random
            // 32-byte refresh token.
            var bytes = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

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
        public IEnumerable<Claim> GetClaims(string jwt, string securityKey)
        {
            // IEnumerable is an interface that the List class implements.
            // LINQ methods can also be used with IEnumerable.
            // An IEnumerable collection can be converted to a List collection
            // by invoking ToList method when needed, or ToArray method
            // to convert the collection to an array.

            // Remove the "Bearer" prefix if exists in the JWT.
            jwt = jwt.StartsWith(JwtBearerDefaults.AuthenticationScheme) ?
                jwt.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length + 1) : 
                jwt;

            // Prepare the signing key and validation parameters.
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(securityKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey
            };

            // Validate the JWT and extract claims then return the claims.
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = jwtSecurityTokenHandler.ValidateToken(
                jwt, tokenValidationParameters, out securityToken);
            // out and ref are used to pass arguments by reference,
            // allowing the method to modify the value of the argument
            // and return it to the caller through the variable (securityToken).
            // Mostly used with value types.
            return securityToken is null ? null : principal.Claims;
        }
    }
}
