using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    // Delete operation will be performed by the Id property inherited
    // from the Record base class. Therefore, no additional properties
    // are needed to be defined.
    public class UserRemoveRequest : Record, IRequest<CommandResponse>
    {
    }

    // UserRemoveHandler class injects the DbContext instance for
    // querying and deleting an existing user in the Users database
    // table.
    // The Handle method implemented from the IRequestHandler
    // interface gets the User entity by ID first.
    // If found, deletes the User entity from the Users database table.
    public class UserRemoveHandler : DbService<User>, 
        IRequestHandler<UserRemoveRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public UserRemoveHandler(DbContext db) : base(db)
        {
        }

        // Override the DbQuery method of the base DbService class to
        // include the UserRole entities to the query. Therefore
        // during delete operation, the related UserRole entities
        // will be deleted first. Then the user entity will be deleted.
        protected override IQueryable<User> DbQuery()
        {
            return base.DbQuery().Include(u => u.UserRoles);
            // u: User entity delegate.
        }

        // Deletes an existing user from the Users database table
        // with the related UserRole entities if it is found by request's ID.
        public async Task<CommandResponse> Handle(UserRemoveRequest request, 
            CancellationToken cancellationToken)
        {
            // Get the User entity by ID from the Users database table
            // and check whether it is null or not.
            var userEntity = await DbSingleAsync(request.Id, cancellationToken);
            if (userEntity is null)
                return Error("User not found!");

            // Delete the related UserRole entities first.
            DbRemove(userEntity.UserRoles);

            // Then delete the User entity from the Users database table.
            await DbRemoveAsync(userEntity, cancellationToken);

            return Success(userEntity.Id, "User deleted successfully.");
        }
    }
}
