using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using Locations.APP.Features.Cities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Locations.APP.Features.Countries
{
    public class CountryQueryRequest : Request, IRequest<IQueryable<CountryQueryResponse>>
    {
    }

    public class CountryQueryResponse : Response
    {
        public string Name { get; set; }
        public List<CityQueryResponse> Cities { get; set; }
    }

    public class CountryQueryHandler : Service<Country>, IRequestHandler<CountryQueryRequest, IQueryable<CountryQueryResponse>>
    {
        protected override IQueryable<Country> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).Include(country => country.Cities).OrderBy(country => country.Name);
        }

        public CountryQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<CountryQueryResponse>> Handle(CountryQueryRequest request, CancellationToken cancellationToken)
        {
            var query = Query().Select(country => new CountryQueryResponse
            {
                Id = country.Id,
                Guid = country.Guid,
                Name = country.Name,
                Cities = country.Cities.OrderBy(city => city.Name).Select(city => new CityQueryResponse
                {
                    Id = city.Id,
                    Guid = city.Guid,
                    Name = city.Name
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}
