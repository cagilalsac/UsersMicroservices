namespace CORE.Authentication.Models
{
    /// <summary>
    /// Represents the response to a JwtRequest or JwtRefreshRequest, 
    /// including the JWT (JSON Web Token) and refresh token.
    /// </summary>
    public class JwtResponse
    {
        /// <summary>
        /// The generated JWT (access token).
        /// </summary>
        public string Jwt { get; set; }

        /// <summary>
        /// Gets or sets the refresh token assigned to the user.
        /// This token is used to request a new JWT without 
        /// requiring re-authentication, typically after the 
        /// original JWT has expired.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
