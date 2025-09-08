using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Locations.APP.Features.Cities
{
    public class CityDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class CityDeleteHandler : Service<City>, IRequestHandler<CityDeleteRequest, CommandResponse>
    {
        public CityDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(CityDeleteRequest request, CancellationToken cancellationToken)
        {
            // isNoTracking is false for being tracked by EF Core to delete the entity
            var entity = await Query(false).SingleOrDefaultAsync(city => city.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("City not found!");

            Delete(entity);

            return Success("City deleted successfully.", entity.Id);
        }
    }
}
