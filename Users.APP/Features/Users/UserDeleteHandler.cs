using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class UserDeleteHandler : Service<User>, IRequestHandler<UserDeleteRequest, CommandResponse>
    {
        public UserDeleteHandler(DbContext db) : base(db)
        {
        }

        // Since User entity has relational UserRole entities, we first need to delete the relational UserRoles in the Handle method.
        // Therefore, we include the UserRoles to the query to get the relational UserRoles data to be deleted.
        protected override IQueryable<User> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).Include(u => u.UserRoles);
        }

        public async Task<CommandResponse> Handle(UserDeleteRequest request, CancellationToken cancellationToken)
        {
            // u: User entity delegate. Get the User entity by ID from the Users table
            // isNoTracking is false for being tracked by EF Core to delete the entity
            var entity = await Query(false).SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("User not found!");

            // delete the relational UserRole entities data
            Delete(entity.UserRoles); // will remove the relational entities data from the UserRoles DbSet as: _db.UserRoles.RemoveRange(entity.UserRoles)

            // delete the User entity data
            Delete(entity); // will delete the entity from the Users DbSet and since save default parameter's value is true, will save changes to the database

            return Success("User deleted successfully.", entity.Id);
        }
    }
}
