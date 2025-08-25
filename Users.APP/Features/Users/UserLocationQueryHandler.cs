using CORE.APP.Models;
using CORE.APP.Services;
using CORE.APP.Services.HTTP;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    /// Handles user location queries by retrieving user data from the database and enriching it with country and city names
    /// fetched from external APIs. Inherits from <see cref="Service{User}"/> for database operations and implements
    /// <see cref="IRequestHandler{UserLocationQueryRequest, List{UserLocationQueryResponse}}"/> for MediatR request handling.
    /// </summary>
    public class UserLocationQueryHandler : Service<User>, IRequestHandler<UserLocationQueryRequest, List<UserLocationQueryResponse>>
    {
        private readonly HttpServiceBase _httpService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLocationQueryHandler"/> class.
        /// </summary>
        /// <param name="db">The database context used for querying user data.</param>
        /// <param name="httpService">The HTTP service used to fetch country and city data from external APIs.</param>
        public UserLocationQueryHandler(DbContext db, HttpServiceBase httpService) : base(db)
        {
            _httpService = httpService;
        }

        /// <summary>
        /// Handles the user location query request by fetching user data from the database and enriching it
        /// with country and city names obtained from external APIs.
        /// </summary>
        /// <param name="request">The request containing API URLs for countries and cities.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A list of <see cref="UserLocationQueryResponse"/> objects with user and location details.</returns>
        public async Task<List<UserLocationQueryResponse>> Handle(UserLocationQueryRequest request, CancellationToken cancellationToken)
        {
            // Fetch the list of countries from the external countries API.
            // Each CountryApiResponse contains country details such as Id and CountryName.
            var countries = await _httpService.GetFromJson<CountryApiResponse>(request.CountriesApiUrl, cancellationToken);

            // Fetch the list of cities from the external cities API.
            // Each CityApiResponse contains city details such as Id and CityName.
            var cities = await _httpService.GetFromJson<CityApiResponse>(request.CitiesApiUrl, cancellationToken);

            // Query the users from the database and project them into UserLocationQueryResponse objects.
            // Only user entity properties with city and country ID properties are set at this stage.
            var users = await Query().Select(userEntity => new UserLocationQueryResponse
            {
                Id = userEntity.Id,
                Guid = userEntity.Guid,
                UserName = userEntity.UserName,
                FullName = userEntity.FirstName + " " + userEntity.LastName,
                Address = userEntity.Address,
                CityId = userEntity.CityId,
                CountryId = userEntity.CountryId
            }).ToListAsync(cancellationToken);

            // For each user, enrich the response with the corresponding city and country names
            // by matching the user's CityId and CountryId with the fetched city and country lists from external APIs.
            foreach (var user in users)
            {
                // Find the city name for the user's CityId; set to empty string if not found.
                user.City = cities.FirstOrDefault(city => city.Id == user.CityId) == null ? string.Empty
                    : cities.FirstOrDefault(city => city.Id == user.CityId).CityName;

                // Find the country name for the user's CountryId; set to empty string if not found.
                user.Country = countries.FirstOrDefault(country => country.Id == user.CountryId) == null ? string.Empty
                    : countries.FirstOrDefault(country => country.Id == user.CountryId).CountryName;
            }

            // Return the list of enriched user location responses.
            return users;
        }
    }
}