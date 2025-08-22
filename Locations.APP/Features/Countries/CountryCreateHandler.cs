using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Locations.APP.Features.Countries
{
    public class CountryCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(125)]
        public string Name { get; set; }
    }

    public class CityCreateHandler : Service<Country>, IRequestHandler<CountryCreateRequest, CommandResponse>
    {
        public CityCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(CountryCreateRequest request, CancellationToken cancellationToken)
        {
            if (await Query().AnyAsync(country => country.CountryName.ToUpper() == request.Name.ToUpper().Trim(), cancellationToken))
                return Error("Country with the same name exists!");

            var entity = new Country
            {
                CountryName = request.Name.Trim()
            };

            Create(entity);

            return Success("Country created successfully.", entity.Id);
        }
    }
}
