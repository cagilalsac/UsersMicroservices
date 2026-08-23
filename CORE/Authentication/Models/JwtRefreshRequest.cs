using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CORE.Authentication.Models
{
    /// <summary>
    /// Represents the request for refreshing a JWT 
    /// (JSON Web Token). Contains both the JWT and 
    /// the refresh token to obtain a new JWT.
    /// </summary>
    public class JwtRefreshRequest
    {
        /// <summary>
        /// The expired or soon-to-expire JWT that 
        /// needs refreshing.
        /// </summary>
        [Required]
        public string Jwt { get; set; }

        /// <summary>
        /// The refresh token used to validate the 
        /// user and issue a new JWT.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the security key used for signing 
        /// or validating JWT. 
        /// This property is ignored during JSON serialization 
        /// and deserialization.
        /// The [JsonIgnore] attribute ensures that the security 
        /// key is not exposed in JSON payloads sent to or 
        /// received from clients.
        /// Typically, this value is assigned from application's 
        /// configuration through IConfiguration instance injection 
        /// and used for cryptographic operations in JWT handling.
        /// </summary>
        [JsonIgnore]
        public string SecurityKey { get; set; }

        /// <summary>
        /// Gets or sets the issuer (iss) claim for the JWT. 
        /// This value identifies the principal (generally API 
        /// server application) that issued the JWT.
        /// The [JsonIgnore] attribute ensures that the issuer 
        /// is not exposed in JSON payloads sent to or received 
        /// from clients.
        /// Typically, this value is assigned from application's 
        /// configuration through IConfiguration instance injection 
        /// and used during JWT generation and validation.
        /// </summary>
        [JsonIgnore]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the audience (aud) claim for the JWT. 
        /// This value identifies the recipients (generally client 
        /// applications) that the JWT is intended for.
        /// The [JsonIgnore] attribute ensures that the audience 
        /// is not exposed in JSON payloads sent to or received 
        /// from clients.
        /// Typically, this value is assigned from application's 
        /// configuration through IConfiguration instance injection 
        /// and used during JWT generation and validation.
        /// </summary>
        [JsonIgnore]
        public string Audience { get; set; }
    }
}
