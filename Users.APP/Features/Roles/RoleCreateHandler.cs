using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    // request properties are created according to the data that will be retrieved from APIs or UIs
    public class RoleCreateRequest : Request, IRequest<CommandResponse>
    {
        // copy all the non navigation properties from Role entity
        [Required, StringLength(25)]
        public string Name { get; set; }

        // since we won't create the relational user data (UserRoles) through this request, we don't need to include the UserIds property here
    }

    public class RoleCreateHandler : Service<Role>, IRequestHandler<RoleCreateRequest, CommandResponse>
    {
        public RoleCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(RoleCreateRequest request, CancellationToken cancellationToken)
        {
            // r: Role entity delegate. Check if a role with the same name exists.
            if (await Query().AnyAsync(r => r.Name.ToUpper() == request.Name.ToUpper().Trim(), cancellationToken))
                return Error("Role with the same name exists!");

            var entity = new Role()
            {
                Name = request.Name.Trim() // request.Name is required and can't be null
            };

            Create(entity); // will add the entity to the Roles DbSet and since save default parameter's value is true, will save changes to the database

            return Success("Role created successfully.", entity.Id);
        }
    }
}
