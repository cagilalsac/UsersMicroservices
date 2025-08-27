using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    /// <summary>
    /// Represents a request model (DTO: Data Trasfer Object) to update an existing Group entity.
    /// Inherits from <see cref="Request"/> and implements <see cref="IRequest{CommandResponse}"/> for MediatR integration.
    /// </summary>
    public class GroupUpdateRequest : Request, IRequest<CommandResponse>
    {
        /// <summary>
        /// The new title for the group.
        /// This property is required and limited to 100 characters.
        /// Data annotations ensure validation at the model binding level.
        /// </summary>
        [Required, StringLength(100)]
        public string Title { get; set; }
    }

    /// <summary>
    /// Handles the update operation for an existing group if any group excluding the current updated one doesn't exist.
    /// Inherits from <see cref="ServiceBase"/> to utilize common service functionality such as 
    /// culture management and returning success / error command responses.
    /// Implements <see cref="IRequestHandler{GroupUpdateRequest, CommandResponse}"/> for MediatR request handling.
    /// </summary>
    public class GroupUpdateHandler : ServiceBase, IRequestHandler<GroupUpdateRequest, CommandResponse>
    {
        private readonly UsersDb _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupUpdateHandler"/> class.
        /// </summary>
        /// <param name="db">The database context for accessing group entity data.</param>
        public GroupUpdateHandler(UsersDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Handles the update logic for an existing group.
        /// Checks for duplicate group titles (case-insensitive, trimmed), ensures the group with same title
        /// excluding the current updated one doesn't exist, and updates its title.
        /// Returns a <see cref="CommandResponse"/> indicating success or error with a result message and optionally entity ID.
        /// </summary>
        /// <param name="request">The group update request containing the group ID and new title.</param>
        /// <param name="cancellationToken">Token for cancelling the async operation.</param>
        /// <returns>A <see cref="CommandResponse"/> with the result of the operation.</returns>
        public async Task<CommandResponse> Handle(GroupUpdateRequest request, CancellationToken cancellationToken)
        {
            // If any other group (excluding the current one) already has the same title (case-insensitive, trimmed), don't update.
            // This prevents duplicate group names in the database.
            // ToLower string method can also be used instead of ToUpper.
            if (await _db.Groups.AnyAsync(groupEntity => groupEntity.Id != request.Id
                && groupEntity.Title.ToUpper() == request.Title.ToUpper().Trim(), cancellationToken))
                return Error("Group with the same title exists!");

            // Attempt to find the group entity by its ID, if not found return error command respnse with message.
            var entity = await _db.Groups.FindAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Group not found!");

            // Update the group's title with the new value (trimmed for consistency).
            entity.Title = request.Title.Trim(); // since request.Title has required data annotation and can't be null, assign request.Title's trimmed value

            // Mark the entity as modified in the context.
            _db.Groups.Update(entity); // _db.Update(entity); can also be written

            // Persist the changes to the database asynchronously by using Unit of Work (all changes made to the DbSets will be commited to the database once).
            await _db.SaveChangesAsync(cancellationToken);

            // Return a success response indicating the group was updated, including the entity's ID.
            return Success("Group updated successfully.", entity.Id);
        }
    }
}
