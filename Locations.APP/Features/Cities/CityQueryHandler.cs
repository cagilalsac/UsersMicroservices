using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using Locations.APP.Features.Countries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Locations.APP.Features.Cities
{
    public class CityQueryRequest : Request, IRequest<IQueryable<CityQueryResponse>>
    {
        public int? CountryId { get; set; }
    }

    public class CityQueryResponse : Response
    {
        public string CityName { get; set; }

        public CountryQueryResponse Country { get; set; }
    }

    public class CityQueryHandler : Service<City>, IRequestHandler<CityQueryRequest, IQueryable<CityQueryResponse>>
    {
        protected override IQueryable<City> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).Include(city => city.Country).OrderBy(city => city.CityName);
        }

        public CityQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<CityQueryResponse>> Handle(CityQueryRequest request, CancellationToken cancellationToken)
        {
            var entityQuery = Query();

            if (request.CountryId.HasValue)
                entityQuery = entityQuery.Where(city => city.CountryId == request.CountryId.Value);

            var query = entityQuery.Select(city => new CityQueryResponse
            {
                Id = city.Id,
                Guid = city.Guid,
                CityName = city.CityName,
                Country = new CountryQueryResponse
                {
                    Id = city.Country.Id,
                    Guid = city.Country.Guid,
                    CountryName = city.Country.CountryName
                }
            });

            return Task.FromResult(query);
        }
    }
}
