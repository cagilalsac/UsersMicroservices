using CORE.Domain;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    // Request and response model classes are also called
    // Data Transfer Object (DTO) classes.
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    // This class represents a MediatR request for querying roles.
    // Inherits from Record and specifies the expected response
    // type as IQueryable of RoleQueryResponse to be presented 
    // as a list or a single item in APIs.
    // This class inherits from Record to include the common
    // identifier property (Id).
    public class RoleQueryRequest : Record, 
        IRequest<IQueryable<RoleQueryResponse>>
    {
        // Optionally properties may be defined here for
        // filtering, ordering or pagination.
    }

    // Represents the response model (DTO: Data Trasfer Object) 
    // for querying Role entities.
    // The properties of a model are generally copied from the 
    // related entity properties which are not navigation properties, 
    // or which have the columns in the related database table.
    // Inherits from Record to include the common identifier
    // property (Id).
    public class RoleQueryResponse : Record
    {
        // Copy all the non navigation properties from Role entity.
        public string Name { get; set; }
    }

    // Inherits the DbService class for Role entity type,
    // therefore Role entity database operations of the DbService
    // class can be used in this class.
    // This class also implements the Mediator interface for types
    // RoleQueryRequest and IQueryable of RoleQueryResponse, therefore
    // the Handler method can be implemented for these types
    // for the role query business operation.
    public class RoleQueryHandler : DbService<Role>,
        IRequestHandler<RoleQueryRequest, IQueryable<RoleQueryResponse>>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public RoleQueryHandler(DbContext db) : base(db)
        {
        }

        // Gets the Role entity query then projects the Role
        // entity query to the RoleQueryResponse query and returns it.
        // Example query: "select Name from Roles".
        // where Name is the RoleQueryResponse property.
        // The returned query can be executed by invoking ToListAsync,
        // SingleOrDefaultAsync, FirstOrDefaultAsync, AnyAsync, etc. LINQ
        // (Language Integrated Query) methods and then the returned
        // result can be used.
        public Task<IQueryable<RoleQueryResponse>> Handle(
            RoleQueryRequest request, CancellationToken cancellationToken)
        {
            // Way 1:
            //IQueryable<RoleQueryResponse> query =
            // Way 2: var can be used instead of any type if
            // there is an assignment.
            var query = 
                DbQuery()// "select * from Roles" entity query.
                .Select(roleEntity => new RoleQueryResponse()
                // () after the class name may not be written.
                {
                    Id = roleEntity.Id,
                    Name = roleEntity.Name
                }); // Map each Role entity property
                    // to RoleQueryResponse property.

            // Return the task to be awaited by the caller to get the
            // IQueryable of type RoleQueryResponse query.
            return Task.FromResult(query);
        }
    }
}
