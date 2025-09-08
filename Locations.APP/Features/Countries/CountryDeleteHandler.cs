using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Locations.APP.Features.Countries
{
    public class CountryDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class CityDeleteHandler : Service<Country>, IRequestHandler<CountryDeleteRequest, CommandResponse>
    {
        protected override IQueryable<Country> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).Include(country => country.Cities);
        }

        public CityDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(CountryDeleteRequest request, CancellationToken cancellationToken)
        {
            // isNoTracking is false for being tracked by EF Core to delete the entity
            var entity = await Query(false).SingleOrDefaultAsync(country => country.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Country not found!");

            // Way 1: check relational cities
            //if (entity.Cities.Any())
            //    return Error("Country can't be deleted because it has relational cities!");
            // Way 2: delete relational cities (not recommended)
            Delete(entity.Cities);

            Delete(entity);

            return Success("Country deleted successfully.", entity.Id);
        }
    }
}
