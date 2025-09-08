using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Locations.APP.Features.Cities
{
    public class CityUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(175)]
        public string CityName { get; set; }

        public int CountryId { get; set; }
    }

    public class CityUpdateHandler : Service<City>, IRequestHandler<CityUpdateRequest, CommandResponse>
    {
        public CityUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(CityUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await Query().AnyAsync(city => city.Id != request.Id && city.CityName == request.CityName.Trim(), cancellationToken))
                return Error("City with the same name exists!");

            // isNoTracking is false for being tracked by EF Core to update the entity
            var entity = await Query(false).SingleOrDefaultAsync(city => city.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("City not found!");

            entity.CityName = request.CityName.Trim();
            entity.CountryId = request.CountryId;

            Update(entity);

            return Success("City updated successfully.", entity.Id);
        }
    }
}
