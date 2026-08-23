using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    // Request properties are created according to the data that
    // will be retrieved from APIs. 
    public class RoleUpdateRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Role entity.
        // Name can't be null and can be maximum 25 characters.
        [Required, StringLength(25)]
        public string Name { get; set; }

        // Modify (1 to many) or add (many to many) the relationship
        // properties. Since we won't update the relational user
        // data (UserRoles) through the request, we don't need the
        // UserIds property here.
        // Required may not be defined if a role may not have a user.
        // For roles-users many to many relationship.
        //[Required]
        //[DisplayName("Users")]
        //public List<int> UserIds { get; set; } = new();
    }

    // RoleUpdateHandler class injects the DbContext instance for
    // querying and updating an existing role in the Roles database
    // table.
    // The Handle method implemented from the IRequestHandler
    // interface checks whether the role with the same trimmed and
    // case sensitive name other than the role request record
    // exists in the Roles database table first.
    // If it doesn't exist, updates the Role entity mapped from the
    // RoleUpdateRequest instance in the Roles database table.
    public class RoleUpdateHandler : DbService<Role>, 
        IRequestHandler<RoleUpdateRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public RoleUpdateHandler(DbContext db) : base(db)
        {
        }

        // Updates an existing role in the Roles database table if a
        // role with the same name other than the role request record
        // doesn't exist.
        public async Task<CommandResponse> Handle(RoleUpdateRequest request, 
            CancellationToken cancellationToken)
        {
            if (await DbQuery()
                .AnyAsync(roleEntity => roleEntity.Id != request.Id &&
                    roleEntity.Name == request.Name.Trim(),
                        cancellationToken))
                return Error($"{request.Name} role exists!");
            // $ with strings can be used for concatenation with { and }.

            // Get the Role entity by ID from the Roles database table
            // and check whether it is null or not.
            var roleEntity = await DbSingleAsync(request.Id, cancellationToken);
            if (roleEntity is null)
                return Error("Role not found!");

            // Update Role entity properties from RoleUpdateRequest properties.
            // Then update the entity in the Roles database table (third save
            // default parameter of the DbUpdateAsync method is true
            // therefore changes of the DbSets are commited to the related
            // database tables).
            roleEntity.Name = request.Name.Trim();
            await DbUpdateAsync(roleEntity, cancellationToken);

            return Success(roleEntity.Id,
                $"{roleEntity.Name} role updated successfully.");
        }
    }
}
