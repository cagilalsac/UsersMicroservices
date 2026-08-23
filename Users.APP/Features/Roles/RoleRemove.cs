using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    // Delete operation will be performed by the Id property inherited
    // from the Record base class. Therefore, no additional properties
    // are needed to be defined.
    public class RoleRemoveRequest : Record, IRequest<CommandResponse>
    {
    }

    // RoleRemoveHandler class injects the DbContext instance for
    // querying and deleting an existing role in the Roles database
    // table.
    // The Handle method implemented from the IRequestHandler
    // interface gets the Role entity by ID first.
    // If found, deletes the Role entity from the Roles database table.
    public class RoleRemoveHandler : DbService<Role>, 
        IRequestHandler<RoleRemoveRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public RoleRemoveHandler(DbContext db) : base(db)
        {
        }

        // Deletes an existing role from the Roles database table if
        // it is found by request's ID.
        public async Task<CommandResponse> Handle(RoleRemoveRequest request, 
            CancellationToken cancellationToken)
        {
            // Get the Role entity by ID from the Roles database table
            // and check whether it is null or not.
            var roleEntity = await DbSingleAsync(request.Id, cancellationToken);
            if (roleEntity is null)
                return Error("Role not found!");

            // Delete the Role entity from the Roles database table
            // (third save default parameter of the DbRemoveAsync method is
            // true therefore changes of the DbSets are commited to
            // the related database tables).
            // Since the delete rule of the Roles and UserRoles relation
            // in the database is Cascade, the relational UserRole entities
            // will be deleted automatically (not recommended).
            await DbRemoveAsync(roleEntity, cancellationToken);

            return Success(roleEntity.Id,
                $"{roleEntity.Name} role deleted successfully.");
        }
    }
}
