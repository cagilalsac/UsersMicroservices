﻿using CORE.APP.Models;
using CORE.APP.Services;
using Locations.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Locations.APP.Features.Cities
{
    public class CityCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(175)]
        public string Name { get; set; }

        public int CountryId { get; set; }
    }

    public class CityCreateHandler : Service<City>, IRequestHandler<CityCreateRequest, CommandResponse>
    {
        public CityCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(CityCreateRequest request, CancellationToken cancellationToken)
        {
            if (await Query().AnyAsync(city => city.Name.ToUpper() == request.Name.ToUpper().Trim(), cancellationToken))
                return Error("City with the same name exists!");

            var entity = new City
            {
                Name = request.Name.Trim(),
                CountryId = request.CountryId
            };

            Create(entity);

            return Success("City created successfully.", entity.Id);
        }
    }
}
