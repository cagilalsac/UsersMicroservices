using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Locations.APP.Features.Countries
{
    public class CountryUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(125)]
        public string Name { get; set; }
    }

    public class CountryUpdateHandler : Service<Country>, IRequestHandler<CountryUpdateRequest, CommandResponse>
    {
        public CountryUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(CountryUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await Query().AnyAsync(country => country.Id != request.Id && country.Name.ToUpper() == request.Name.ToUpper().Trim(), cancellationToken))
                return Error("Country with the same name exists!");

            var entity = await Query().SingleOrDefaultAsync(country => country.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Country not found!");

            entity.Name = request.Name.Trim();

            Update(entity);

            return Success("Country updated successfully.", entity.Id);
        }
    }
}
