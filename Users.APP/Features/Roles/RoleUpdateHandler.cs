using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    // request properties are created according to the data that will be retrieved from APIs or UIs
    public class RoleUpdateRequest : Request, IRequest<CommandResponse>
    {
        // copy all the non navigation properties from Role entity
        [Required, StringLength(25)]
        public string Name { get; set; }

        // since we won't update the relational user data (UserRoles) through this request, we don't need to include the UserIds property here
    }

    public class RoleUpdateHandler : Service<Role>, IRequestHandler<RoleUpdateRequest, CommandResponse>
    {
        public RoleUpdateHandler(DbContext db) : base(db)
        {
        }

        // No need to override the Query method to include the relational UserRole entities because we are not updating
        // the UserIds of the role. Therefore, we don't have to delete the relational UserRole entities.

        public async Task<CommandResponse> Handle(RoleUpdateRequest request, CancellationToken cancellationToken)
        {
            // r: Role entity delegate. Check if a role excluding the current updated role with the same name exists.
            if (await Query().AnyAsync(r => r.Id != request.Id && r.Name.ToUpper() == request.Name.ToUpper().Trim(), cancellationToken))
                return Error("Role with the same name exists!");

            // get the Role entity by ID from the Roles table
            var entity = await Query().SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Role not found!");

            // update retrieved Role entity's properties with request properties
            entity.Name = request.Name.Trim(); // request.Name is required and can't be null
            
            Update(entity); // will update the entity in the Roles DbSet and since save default parameter's value is true, will save changes to the database

            return Success("Role updated successfully.", entity.Id);
        }
    }
}
