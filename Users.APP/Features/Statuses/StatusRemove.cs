using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Statuses
{
    // Delete operation will be performed by the Id property inherited
    // from the Record base class. Therefore, no additional properties
    // are needed to be defined.
    public class StatusRemoveRequest : Record, IRequest<CommandResponse>
    {
    }

    // StatusRemoveHandler class injects the DbContext instance for
    // querying and deleting an existing status in the Statuses database
    // table.
    // The Handle method implemented from the IRequestHandler
    // interface gets the Status entity by ID first.
    // If found, first checks if there are any related Users and
    // if none, deletes the Status entity from the Statuses database table.
    public class StatusRemoveHandler : DbService<Status>,
        IRequestHandler<StatusRemoveRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public StatusRemoveHandler(DbContext db) : base(db)
        {
        }

        // Override the DbQuery method to include the related Users
        // to the Status entity query.
        protected override IQueryable<Status> DbQuery()
        {
            return base.DbQuery().Include(statusEntity => statusEntity.Users);
        }

        // Deletes an existing status from the Statuses database table if
        // it is found by request's ID and it has no related users.
        public async Task<CommandResponse> Handle(StatusRemoveRequest request,
            CancellationToken cancellationToken)
        {
            // Get the Status entity by ID from the Statuses database table
            // and check whether it is null or not.
            var statusEntity = await DbSingleAsync(request.Id, cancellationToken);
            if (statusEntity is null)
                return Error("Status not found!");

            // Check if there are any related Users for the Status entity.
            if (statusEntity.Users.Any()) // if (statusEntity.Users.Count > 0)
                                          // can also be written.
                return Error("Status has relational users!");

            // Delete the Status entity from the Statuses database table
            // (third save default parameter of the DbRemoveAsync method is
            // true therefore changes of the DbSets are commited to
            // the related database tables).
            await DbRemoveAsync(statusEntity, cancellationToken);

            return Success(statusEntity.Id, "Status deleted successfully.");
        }
    }
}
