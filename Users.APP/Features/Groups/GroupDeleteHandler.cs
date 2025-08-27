using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    /// <summary>
    /// Represents a request model to delete a Group entity.
    /// Inherits from the base <see cref="Request"/> class and implements <see cref="IRequest{CommandResponse}"/>.
    /// </summary>
    public class GroupDeleteRequest : Request, IRequest<CommandResponse>
    {
        // No additional properties are needed; the base Request.Id is used to specify the group to delete.
    }

    /// <summary>
    /// Handles the deletion of a group entity from the database.
    /// Inherits from <see cref="ServiceBase"/> to provide culture and command response helpers,
    /// and implements <see cref="IRequestHandler{GroupDeleteRequest, CommandResponse}"/> for MediatR pipeline integration.
    /// </summary>
    public class GroupDeleteHandler : ServiceBase, IRequestHandler<GroupDeleteRequest, CommandResponse>
    {
        // The database context used to access and manipulate group entities.
        private readonly UsersDb _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDeleteHandler"/> class.
        /// </summary>
        /// <param name="db">The database context for user-related entities.</param>
        public GroupDeleteHandler(UsersDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Handles the group deletion request.
        /// </summary>
        /// <param name="request">The request containing the ID of the group to delete.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="CommandResponse"/> indicating the result of the operation:
        /// - Success if the group was found and deleted.
        /// - Error if the group was not found.
        /// </returns>
        public async Task<CommandResponse> Handle(GroupDeleteRequest request, CancellationToken cancellationToken)
        {
            // Before the implementation of User entity: Attempt to find the group entity by its ID.
            //var entity = await _db.Groups.FindAsync(request.Id, cancellationToken);
            // After the implementation of User entity: Get the Group entity with relational User entities data by ID.
            var entity = await _db.Groups.Include(groupEntity => groupEntity.Users)
                .SingleOrDefaultAsync(groupEntity => groupEntity.Id == request.Id, cancellationToken);

            // If the group does not exist, return an error command response.
            if (entity is null)
                return Error("Group not found!");

            // After the implementation of User entity: Check if there are any relational User entities data of the group.
            if (entity.Users.Count > 0) // if (entity.Users.Any()) can also be written
                return Error("Group can't be deleted because it has relational users!"); // don't delete the group and return error command response

            // Remove the group entity from the database context.
            _db.Groups.Remove(entity); // _db.Remove(entity); can also be written

            // Persist the changes to the database asynchronously by using Unit of Work (all changes made to the DbSets will be commited to the database once).
            await _db.SaveChangesAsync(cancellationToken);

            // Return a success response indicating the group was deleted with the deleted group entity's ID value.
            return Success("Group deleted successfully.", entity.Id);
        }
    }
}
