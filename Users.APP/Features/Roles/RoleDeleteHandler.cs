using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    public class RoleDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class RoleDeleteHandler : Service<Role>, IRequestHandler<RoleDeleteRequest, CommandResponse>
    {
        public RoleDeleteHandler(DbContext db) : base(db)
        {
        }

        // Since Role entity has relational UserRole entities, we first need to delete the relational UserRoles in the Handle method.
        // Therefore, we include the UserRoles to the query to get the relational UserRoles data to be deleted.
        protected override IQueryable<Role> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).Include(r => r.UserRoles);
        }

        public async Task<CommandResponse> Handle(RoleDeleteRequest request, CancellationToken cancellationToken)
        {
            // r: Role entity delegate. Get the Role entity by ID from the Roles table
            var entity = await Query().SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Role not found!");

            // delete the relational UserRole entities data
            Delete(entity.UserRoles); // will remove the relational entities data from the UserRoles DbSet as: _db.UserRoles.RemoveRange(entity.UserRoles)

            // delete the Role entity data
            Delete(entity); // will delete the entity from the Roles DbSet and since save default parameter's value is true, will save changes to the database

            return Success("Role deleted successfully.", entity.Id);
        }
    }
}
