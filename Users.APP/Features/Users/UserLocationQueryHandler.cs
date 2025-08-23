using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    /// <summary>
    /// Request object for querying user locations, including API URLs for countries and cities.
    /// </summary>
    public class UserLocationQueryRequest : Request, IRequest<List<UserLocationQueryResponse>>
    {
        //This property is ignored during JSON serialization therefore will not be assigned with the request, will be assigned at the controller's action.
        [JsonIgnore]
        public string CountriesApiUrl { get; set; }

        // This property is ignored during JSON serialization therefore will not be assigned with the request, will be assigned at the controller's action.
        [JsonIgnore]
        public string CitiesApiUrl { get; set; }
    }

    /// <summary>
    /// Response object representing user location details.
    /// </summary>
    public class UserLocationQueryResponse : Response
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public int? CityId { get; set; }
        public string City { get; set; }
    }

    /// <summary>
    /// Represents a response from the countries API.
    /// </summary>
    public class CountryApiResponse : Response
    {
        public string CountryName { get; set; }
    }

    /// <summary>
    /// Represents a response from the cities API.
    /// </summary>
    public class CityApiResponse : Response
    {
        public string CityName { get; set; }
    }

    /// <summary>
    /// Handles user location queries by retrieving user data and enriching it with country and city names from external APIs.
    /// </summary>
    public class UserLocationQueryHandler : Service<User>, IRequestHandler<UserLocationQueryRequest, List<UserLocationQueryResponse>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLocationQueryHandler"/> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for retrieving request headers.</param>
        /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
        public UserLocationQueryHandler(DbContext db, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory) : base(db)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Handles the user location query request by fetching users and enriching their data with country and city names from external APIs.
        /// </summary>
        /// <param name="request">The user location query request containing API URLs.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A list of <see cref="UserLocationQueryResponse"/> objects with user and location details.</returns>
        public async Task<List<UserLocationQueryResponse>> Handle(UserLocationQueryRequest request, CancellationToken cancellationToken)
        {
            // Initialize the result list
            var users = new List<UserLocationQueryResponse>();

            // Prepare variables for API tokens and HTTP clients
            string countriesApiToken, citiesApiToken;
            HttpClient countriesHttpClient, citiesHttpClient;

            // If either API URL is missing, return an empty list
            if (string.IsNullOrWhiteSpace(request.CountriesApiUrl) || string.IsNullOrWhiteSpace(request.CitiesApiUrl))
                return users;

            // Retrieve the Authorization header of the current request for the countries API
            countriesApiToken = _httpContextAccessor.HttpContext?.Request?.Headers?.Authorization.FirstOrDefault();
            countriesHttpClient = _httpClientFactory.CreateClient();
            if (!string.IsNullOrWhiteSpace(countriesApiToken))
            {
                // Remove the "Bearer" prefix if present
                if (countriesApiToken.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                    countriesApiToken = countriesApiToken.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length).TrimStart();
                // Add the token to the HTTP client Authorization header
                countriesHttpClient.DefaultRequestHeaders.Add("Authorization", countriesApiToken);
            }

            // Retrieve the Authorization header of the current request for the cities API
            citiesApiToken = _httpContextAccessor.HttpContext?.Request?.Headers?.Authorization.FirstOrDefault();
            citiesHttpClient = _httpClientFactory.CreateClient();
            if (!string.IsNullOrWhiteSpace(citiesApiToken))
            {
                // Remove the "Bearer" prefix if present
                if (citiesApiToken.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                    citiesApiToken = citiesApiToken.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length).TrimStart();
                // Add the token to the HTTP client Authorization header
                citiesHttpClient.DefaultRequestHeaders.Add("Authorization", citiesApiToken);
            }

            // Fetch the list of countries from the external API
            var countries = await countriesHttpClient.GetFromJsonAsync<List<CountryApiResponse>>(request.CountriesApiUrl, cancellationToken);
            // Fetch the list of cities from the external API
            var cities = await citiesHttpClient.GetFromJsonAsync<List<CityApiResponse>>(request.CitiesApiUrl, cancellationToken);

            // Query users from the database and project to response objects
            users = await Query().Select(userEntity => new UserLocationQueryResponse
            {
                Id = userEntity.Id,
                Guid = userEntity.Guid,
                UserName = userEntity.UserName,
                FullName = userEntity.FirstName + " " + userEntity.LastName,
                Address = userEntity.Address,
                CityId = userEntity.CityId,
                CountryId = userEntity.CountryId
            }).ToListAsync(cancellationToken);

            // Enrich each user with city and country names from the API responses
            foreach (var user in users)
            {
                // Find the city name by CityId
                user.City = cities.FirstOrDefault(city => city.Id == user.CityId) == null ? string.Empty
                    : cities.FirstOrDefault(city => city.Id == user.CityId).CityName;
                // Find the country name by CountryId
                user.Country = countries.FirstOrDefault(country => country.Id == user.CountryId) == null ? string.Empty
                    : countries.FirstOrDefault(country => country.Id == user.CountryId).CountryName;
            }

            // Return the enriched user list
            return users;
        }
    }
}