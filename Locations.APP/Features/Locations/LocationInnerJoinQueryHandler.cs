using CORE.APP.Extensions;
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
    /// Represents a request for an inner join query between countries and cities, including filtering, ordering, and paging options.
    /// </summary>
    public class LocationInnerJoinQueryRequest : IRequest<IQueryable<LocationInnerJoinQueryResponse>>, 
        IPageRequest, IOrderRequest // Interface Segregation Principle (I of SOLID) is applied
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
        /// Gets or sets the current page number for paging (1-based, default 1).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of records to return per page for paging.
        /// </summary>
        public int CountPerPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of records available for paging (for informational purposes).
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public int TotalCountForPaging { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity property for ordering by, default CountryName (e.g., "CountryName" or "CityName").
        /// </summary>
        public string OrderEntityPropertyName { get; set; } = "CountryName";

        /// <summary>
        /// Gets or sets a value indicating whether the direction is ascending or descending for ordering.
        /// </summary>
        public bool IsOrderDescending { get; set; }
    }

    /// <summary>
    /// Represents the response object for the inner join query between countries and cities.
    /// </summary>
    public class LocationInnerJoinQueryResponse
    {
        /// <summary>
        /// Gets or sets the ID of the country.
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the name of the country.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the city.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        public string CityName { get; set; }
    }

    /// <summary>
    /// Handles the inner join query between countries and cities, supporting filtering, ordering, and paging.
    /// </summary>
    public class LocationInnerJoinQueryHandler : Service<Country>, IRequestHandler<LocationInnerJoinQueryRequest, IQueryable<LocationInnerJoinQueryResponse>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationInnerJoinQueryHandler"/> class.
        /// </summary>
        /// <param name="db">The database context to be used for data access.</param>
        public LocationInnerJoinQueryHandler(DbContext db) : base(db)
        {
            // The base Service class handles the DbContext assignment.
        }

        /// <summary>
        /// Handles the inner join query request for locations, returning a paginated, filtered, and ordered list of country-city pairs.
        /// </summary>
        /// <param name="request">The query request containing filter, paging, and ordering parameters.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IQueryable{LocationInnerJoinQueryResponse}"/> representing the joined, filtered, ordered, and paginated result set.
        /// </returns>
        public Task<IQueryable<LocationInnerJoinQueryResponse>> Handle(LocationInnerJoinQueryRequest request, CancellationToken cancellationToken)
        {
            // Retrieve the queryable collections for countries and cities from the database context.
            var countryQuery = Query();
            var cityQuery = Query<City>();

            // Perform an inner join between countries and cities on the CountryId field.
            // Project the query result into a LocationInnerJoinQueryResponse DTO (Model).
            var innerJoinQuery = from country in countryQuery
                                 join city in cityQuery on country.Id equals city.CountryId
                                 select new LocationInnerJoinQueryResponse
                                 {
                                     CountryId = country.Id,
                                     CountryName = country.CountryName,
                                     CityId = city.Id,
                                     CityName = city.CityName
                                 };

            // Apply ordering based on the requested entity property and direction.
            if (request.OrderEntityPropertyName == nameof(Country.CountryName))
            {
                // Order by country name, descending or ascending.
                if (request.IsOrderDescending)
                    innerJoinQuery = innerJoinQuery.OrderByDescending(location => location.CountryName);
                else
                    innerJoinQuery = innerJoinQuery.OrderBy(location => location.CountryName);
            }
            else if (request.OrderEntityPropertyName == nameof(City.CityName))
            {
                // Order by city name, descending or ascending.
                if (request.IsOrderDescending)
                    innerJoinQuery = innerJoinQuery.OrderByDescending(location => location.CityName);
                else
                    innerJoinQuery = innerJoinQuery.OrderBy(location => location.CityName);
            }

            // Apply filtering by country name if provided in the request.
            // Way 1:
            //if (!string.IsNullOrWhiteSpace(request.CountryName))
            // Way 2: use the string extension method defined in CORE\APP\Extensions\StringExtensions.cs
            if (request.CountryName.HasAny())
            {
                // Filter results to include only those where the country name contains the provided value 
                // (case-sensitive, no white space characters in the beginning and at the end).
                innerJoinQuery = innerJoinQuery.Where(location => location.CountryName.Contains(request.CountryName.Trim()));
            }

            // Apply filtering by city name if provided in the request.
            // Way 1:
            //if (!string.IsNullOrWhiteSpace(request.CityName))
            // Way 2: use the string extension method defined in CORE\APP\Extensions\StringExtensions.cs
            if (request.CityName.HasAny())
            {
                // Filter results to include only those where the city name contains the provided value 
                // (case-sensitive, no white space characters in the beginning and at the end).
                innerJoinQuery = innerJoinQuery.Where(location => location.CityName.Contains(request.CityName.Trim()));
            }

            // Set the total count of filtered records for client-side paging information.
            request.TotalCountForPaging = innerJoinQuery.Count();

            // Apply paging if both PageNumber and CountPerPage are specified and greater than zero.
            if (request.PageNumber > 0 && request.CountPerPage > 0)
            {
                // Calculate the number of records to skip and take for the current page.
                var skipValue = (request.PageNumber - 1) * request.CountPerPage;
                var takeValue = request.CountPerPage;

                // Apply Skip and Take for paging.
                innerJoinQuery = innerJoinQuery.Skip(skipValue).Take(takeValue);
            }

            // Return the composed query as a Task result.
            return Task.FromResult(innerJoinQuery);
        }
    }
}
