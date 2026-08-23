using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Statuses
{
    // Request properties are created according to the data that
    // will be retrieved from APIs. 
    public class StatusUpdateRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Status entity.
        // Title can't be null and can be maximum 5 characters.
        [Required, StringLength(5)]
        public string Title { get; set; }
    }

    // StatusUpdateHandler class injects the DbContext instance for
    // querying and updating an existing status in the Statuses database
    // table.
    // The Handle method implemented from the IRequestHandler
    // interface checks whether the status with the same trimmed and
    // case sensitive title other than the status request record
    // exists in the Statuses database table first.
    // If it doesn't exist, updates the Status entity mapped from the
    // StatusUpdateRequest instance in the Statuses database table.
    public class StatusUpdateHandler : DbService<Status>,
        IRequestHandler<StatusUpdateRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public StatusUpdateHandler(DbContext db) : base(db)
        {
        }

        // Updates an existing status in the Statuses database table if a
        // status with the same title other than the status request record
        // doesn't exist.
        public async Task<CommandResponse> Handle(StatusUpdateRequest request,
            CancellationToken cancellationToken)
        {
            if (await DbQuery()
                .AnyAsync(statusEntity => statusEntity.Id != request.Id &&
                    statusEntity.Title == request.Title.Trim(),
                        cancellationToken))
                return Error("Status with the same title exists!");

            // Get the Status entity by ID from the Statuses database table
            // and check whether it is null or not.
            var statusEntity = await DbSingleAsync(request.Id, cancellationToken);
            if (statusEntity is null)
                return Error("Status not found!");

            // Update Status entity properties from StatusUpdateRequest properties.
            // Then update the entity in the Statuses database table (third save
            // default parameter of the DbUpdateAsync method is true
            // therefore changes of the DbSets are commited to the related
            // database tables).
            statusEntity.Title = request.Title.Trim();
            await DbUpdateAsync(statusEntity, cancellationToken);

            return Success(statusEntity.Id, "Status updated successfully.");
        }
    }
}
