using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    public class RoleQueryRequest : Request, IRequest<IQueryable<RoleQueryResponse>>
    {
    }

    // response properties are created according to the data to be presented in APIs or UIs
    public class RoleQueryResponse : Response
    {
        // copy all the non navigation properties from Role entity
        public string Name { get; set; }



        // add the new properties, some ending with F for the properties with the same name, for custom or formatted string values
        public int UserCount { get; set; }

        public string Users { get; set; }
    }



    // Inherit from the generic entity service class therefore DbContext injected constructor can be automatically created
    // and entity CRUD (create, read, update, delete) methods in the base class can be invoked.
    public class RoleQueryHandler : Service<Role>, IRequestHandler<RoleQueryRequest, IQueryable<RoleQueryResponse>>
    {
        public RoleQueryHandler(DbContext db) : base(db)
        {
        }

        // base virtual Query method is overriden therefore the overriden query can be used in all other methods
        protected override IQueryable<Role> Query(bool isNoTracking = true)
        {
            // r: Role entity delegate, ur: UserRole entity delegate
            return base.Query(isNoTracking) // will return Roles DbSet
                .Include(r => r.UserRoles).ThenInclude(ur => ur.User) // will first include the relational UserRoles then User data
                .OrderBy(r => r.Name); // query will be ordered ascending by Name values

            // Include, ThenInclude, OrderBy, OrderByDescending, ThenBy and ThenByDescending methods can also be used with DbSets.
        }

        public Task<IQueryable<RoleQueryResponse>> Handle(RoleQueryRequest request, CancellationToken cancellationToken)
        {
            var query = Query().Select(r => new RoleQueryResponse()
            {
                // assigning entity properties to the response
                Id = r.Id,
                Guid = r.Guid,
                Name = r.Name,

                // assigning custom or formatted properties to the response
                UserCount = r.UserRoles.Count, // returns the users count of each role
                Users = string.Join(", ", r.UserRoles.Select(ur => ur.User.UserName)) // returns a comma seperated user names string for each role
            });

            return Task.FromResult(query);
        }
    }
}
