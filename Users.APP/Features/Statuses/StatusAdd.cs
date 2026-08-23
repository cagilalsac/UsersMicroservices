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
    public class StatusAddRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Status entity.
        [Required, StringLength(5)]
        public string Title { get; set; }
    }

    // StatusAddHandler class injects the DbContext instance for
    // querying and inserting a new status to the Statuses database table.
    // The Handle method implemented from the IRequestHandler
    // interface checks whether the status with the same trimmed and
    // case sensitive title exists in the Statuses database table first.
    // If it doesn't exist, inserts the Status entity mapped from the
    // StatusAddRequest instance to the Statuses database table.
    public class StatusAddHandler : DbService<Status>, 
        IRequestHandler<StatusAddRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public StatusAddHandler(DbContext db) : base(db)
        {
        }

        // Inserts a new status to the Statuses database table if a
        // status with the same title doesn't exist.
        public async Task<CommandResponse> Handle(StatusAddRequest request, 
            CancellationToken cancellationToken)
        {
            if (await DbQuery()
                .AnyAsync(statusEntity =>
                    statusEntity.Title == request.Title.Trim(),
                        cancellationToken))
                return Error("Status with the same title exists!");
            // Trim method removes white space characters in the
            // beginning and at the end.

            // Create a new Status entity instance from the StatusAddRequest
            // instance then insert the entity in the Statuses database
            // table (third save default parameter of the DbAddAsync
            // method is true therefore changes of the DbSets are
            // commited to the related database tables).
            var statusEntity = new Status
            {
                Title = request.Title.Trim()
            };
            await DbAddAsync(statusEntity, cancellationToken);

            // Return a successful command response with the new
            // status ID.
            // Entity's Id value will be updated by the database
            // after the insert operation.
            return Success(statusEntity.Id, "Status created successfully.");
        }
    }
}
