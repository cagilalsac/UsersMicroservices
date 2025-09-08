using CORE.APP.Extensions;
using CORE.APP.Models;
using CORE.APP.Models.Ordering;
using CORE.APP.Models.Paging;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Locations.APP.Features.Locations
{
    /// <summary>
    /// Represents a request for a left join query between countries and cities, including filtering, ordering, and paging options.
    /// </summary>
    public class LocationLeftJoinQueryRequest : Request, IRequest<IQueryable<LocationLeftJoinQueryResponse>>, IPageRequest, IOrderRequest
    {
        /// <summary>
        /// Gets or sets the country name filter for the query.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the city name filter for the query.
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// Gets or sets the current page number for paging (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of records to return per page for paging.
        /// </summary>
        public int CountPerPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of records available for paging (for informational purposes).
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity property for ordering by (e.g., "CountryName" or "CityName").
        /// </summary>
        public string EntityPropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the direction is ascending for ordering.
        /// </summary>
        public bool IsDescending { get; set; }
    }

    /// <summary>
    /// Represents the response object for the left join query between countries and cities.
    /// </summary>
    public class LocationLeftJoinQueryResponse : Response
    {
        /// <summary>
        /// Gets or sets the integer ID of the response.
        /// We don't need Id as Country ID because we will define a property as CountryId for it. 
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        /// <summary>
        /// Gets or sets the GUID of the response.
        /// We don't need to return country GUID in the response. 
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string Guid { get => base.Guid; set => base.Guid = value; }

        /// <summary>
        /// Gets or sets the ID of the country.
        /// Must be defined nullable for left join in case some countries may not have any associated cities, 
        /// therefore the value of the property may be null.
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// Gets or sets the name of the country.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the city.
        /// Must be defined nullable for left join since the value of the property may be null.
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        public string CityName { get; set; }
    }

    /// <summary>
    /// Handles the left join query between countries and cities, supporting filtering, ordering, and paging.
    /// </summary>
    public class LocationLeftJoinQueryHandler : Service<Country>, IRequestHandler<LocationLeftJoinQueryRequest, IQueryable<LocationLeftJoinQueryResponse>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationLeftJoinQueryHandler"/> class.
        /// </summary>
        /// <param name="db">The database context to be used for data access.</param>
        public LocationLeftJoinQueryHandler(DbContext db) : base(db)
        {
            // The base Service class handles the DbContext assignment.
        }

        /// <summary>
        /// Handles the left join query request for locations, returning a paginated, filtered, and ordered list of country-city pairs
        /// with or without (city property values will be null) cities.
        /// </summary>
        /// <param name="request">The query request containing filter, paging, and ordering parameters.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IQueryable{LocationLeftJoinQueryResponse}"/> representing the joined, filtered, ordered, and paginated result set.
        /// </returns>
        public Task<IQueryable<LocationLeftJoinQueryResponse>> Handle(LocationLeftJoinQueryRequest request, CancellationToken cancellationToken)
        {
            // Retrieve the queryable collections for countries and cities from the database context.
            var countryQuery = Query();
            var cityQuery = Query<City>();

            // Perform a left join between countries and cities on the CountryId field.
            // Project the ordered ascending by country name then ordered ascending by city name query result into a LocationLeftJoinQueryResponse DTO (Model).
            var leftJoinQuery = from country in countryQuery
                                join city in cityQuery on country.Id equals city.CountryId into countryCity
                                from city in countryCity.DefaultIfEmpty() // left join to include countries without cities
                                orderby country.CountryName, city.CityName
                                select new LocationLeftJoinQueryResponse
                                {
                                    CountryId = country.Id,
                                    CountryName = country.CountryName,
                                    CityId = city.Id,
                                    CityName = city.CityName
                                };

            // Apply ordering based on the requested entity property and direction.
            if (request.EntityPropertyName == nameof(Country.CountryName))
            {
                // Order by country name, descending or ascending.
                if (request.IsDescending)
                    leftJoinQuery = leftJoinQuery.OrderByDescending(location => location.CountryName);
                else
                    leftJoinQuery = leftJoinQuery.OrderBy(location => location.CountryName);
            }
            else if (request.EntityPropertyName == nameof(City.CityName))
            {
                // Order by city name, descending or ascending.
                if (request.IsDescending)
                    leftJoinQuery = leftJoinQuery.OrderByDescending(location => location.CityName);
                else
                    leftJoinQuery = leftJoinQuery.OrderBy(location => location.CityName);
            }

            // Apply filtering by country name if provided in the request.
            // Way 1:
            //if (!string.IsNullOrWhiteSpace(request.CountryName))
            // Way 2: use the string extension method defined in APP\Extensions\StringExtensions.cs
            if (request.CountryName.HasAny())
            {
                // Filter results to include only those where the country name contains the provided value
                // (case-sensitive, no white space characters in the beginning and at the end).
                // Because of left join, this query response property value may be null, so we need to use null-coalescing operator (??)
                // for this string query response property reached by the location delegate if we use any string class property or method with it
                // to prevent null reference exception.
                leftJoinQuery = leftJoinQuery.Where(location => (location.CountryName ?? "").Contains(request.CountryName.Trim()));
            }

            // Apply filtering by city name if provided in the request.
            // Way 1:
            //if (!string.IsNullOrWhiteSpace(request.CityName))
            // Way 2: use the string extension method defined in APP\Extensions\StringExtensions.cs
            if (request.CityName.HasAny())
            {
                // Filter results to include only those where the city name contains the provided value
                // (case-sensitive, no white space characters in the beginning and at the end).
                // Because of left join, this query response property value may be null, so we need to use null-coalescing operator (??)
                // for this string query response property reached by the location delegate if we use any string class property or method with it
                // to prevent null reference exception.
                leftJoinQuery = leftJoinQuery.Where(location => (location.CityName ?? "").Contains(request.CityName.Trim()));
            }

            // Apply paging if both PageNumber and CountPerPage are specified and greater than zero.
            if (request.PageNumber > 0 && request.CountPerPage > 0)
            {
                // Set the total count of filtered records for client-side paging information.
                request.TotalCount = leftJoinQuery.Count();

                // Calculate the number of records to skip and take for the current page.
                var skipValue = (request.PageNumber - 1) * request.CountPerPage;
                var takeValue = request.CountPerPage;

                // Apply Skip and Take for paging.
                leftJoinQuery = leftJoinQuery.Skip(skipValue).Take(takeValue);
            }

            // Return the composed query as a Task result.
            return Task.FromResult(leftJoinQuery);
        }
    }
}
