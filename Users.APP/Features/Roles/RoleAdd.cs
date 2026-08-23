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
    public class RoleAddRequest : Record, IRequest<CommandResponse>
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

    // RoleAddHandler class injects the DbContext instance for
    // querying and inserting a new role to the Roles database table.
    // The Handle method implemented from the IRequestHandler
    // interface checks whether the role with the same trimmed and
    // case sensitive name exists in the Roles database table first.
    // If it doesn't exist, inserts the Role entity mapped from the
    // RoleAddRequest instance to the Roles database table.
    public class RoleAddHandler : DbService<Role>, 
        IRequestHandler<RoleAddRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public RoleAddHandler(DbContext db) : base(db)
        {
        }

        // Inserts a new role to the Roles database table if a
        // role with the same name doesn't exist.
        public async Task<CommandResponse> Handle(RoleAddRequest request, 
            CancellationToken cancellationToken)
        {
            if (await DbQuery()
                .AnyAsync(roleEntity =>
                    roleEntity.Name == request.Name.Trim(), 
                        cancellationToken))
                return Error(request.Name + " role exists!");
            // Trim method removes white space characters in the
            // beginning and at the end.

            // Create a new Role entity instance from the RoleAddRequest
            // instance then insert the entity in the Roles database
            // table (third save default parameter of the DbAddAsync
            // method is true therefore changes of the DbSets are
            // commited to the related database tables).
            var roleEntity = new Role
            {
                Name = request.Name.Trim()
            };
            await DbAddAsync(roleEntity, cancellationToken);

            // Return a successful command response with the new
            // role's ID.
            // Entity's Id value will be updated by the database
            // after the insert operation.
            return Success(roleEntity.Id, roleEntity.Name + 
                " role created successfully.");
        }
    }
}
