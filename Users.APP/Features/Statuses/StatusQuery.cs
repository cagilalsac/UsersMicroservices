using CORE.Domain;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Users;

namespace Users.APP.Features.Statuses
{
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    public class StatusQueryRequest : Record,
        IRequest<IQueryable<StatusQueryResponse>>
    {
    }

    // Response properties are created according to the data to be
    // presented in API responses.
    public class StatusQueryResponse : Record
    {
        // Copy all the non navigation properties from Status entity.
        public string Title { get; set; }



        // Add the new properties ending with R declaring Response
        // for custom properties or formatted string value properties.
        // Way 1: Map primitive type properties for basic information.
        public string UserNamesR { get; set; }

        // Property value for the count of the users of the status.
        public int UserCountR { get; set; }

        // Way 2: Map complex type object for more information.
        // Either Way 1, Way 2 or both can be used.
        public List<UserQueryResponse> UsersR { get; set; }
    }

    // StatusQueryHandler inherits the DbService class for Status entity type,
    // therefore Status entity database operations of the DbService
    // class can be used in this class.
    // StatusQueryHandler also implements the Mediator IRequestHandler
    // interface for StatusQueryRequest and IQueryable of StatusQueryResponse
    // types, therefore IRequestHandler interface's Handle method
    // definition can be implemented for types StatusQueryRequest and
    // IQueryable of StatusQueryResponse in this class, which will be used by
    // the related controller's action.
    public class StatusQueryHandler : DbService<Status>,
        IRequestHandler<StatusQueryRequest, IQueryable<StatusQueryResponse>>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public StatusQueryHandler(DbContext db) : base(db)
        {
        }

        // Overrides the base virtual DbQuery method to include
        // User entities data to the query to be used in the Handle
        // method and orders the query by Status entity Title
        // property ascending. 
        // The overridden DbQuery method can be used in any method
        // below by invoking the DbQuery method.
        protected override IQueryable<Status> DbQuery()
        {
            return base.DbQuery() // "select * from Statuses"
                                  // entity query.
                .Include(s => s.Users) // Includes the relational
                                       // User entities data to the query
                                       // by using left outer join.
                .OrderBy(s => s.Title); // Orders the query by Title
                                        // property of the Status entity
                                        // in ascending order.
                                        // OrderByDescending can be used
                                        // for descending order.
            // s: Status entity delegate.
        }

        // Gets the updated query from the DbQuery method then projects
        // the Status entities to status query responses and returns the
        // StatusQueryResponse query.
        // The returned query can be executed by invoking ToListAsync,
        // SingleOrDefaultAsync, FirstOrDefaultAsync, AnyAsync, etc.
        // LINQ (Language Integrated Query) methods and then the
        // returned result can be used.
        public Task<IQueryable<StatusQueryResponse>> Handle(
            StatusQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbQuery().Select(s => new StatusQueryResponse
            {
                // Map each Status entity property to
                // StatusQueryResponse property.
                Id = s.Id,
                Title = s.Title,
                // Way 1: Map primitive type data for basic information.
                UserNamesR = string.Join(", ", s.Users.Select(u => u.UserName)),
                // string type's Join method gets a seperator as the first
                // parameter and gets a collection of strings as the second
                // parameter then concatenates each string item in collection
                // with the seperator and returns a string.
                UserCountR = s.Users.Count,
                // Count property of the List type returns the item count 
                // of the collection.
                // Way 2: Map complex type object for more information.
                // Either Way 1, Way 2 or both can be used.
                UsersR = s.Users.Select(u => new UserQueryResponse
                {
                    // Map each User entity property to
                    // StatusQueryResponse’s UserQueryResponse property.
                    Id = u.Id,
                    UserName = u.UserName,
                    PasswordR = new string('*', u.Password.Length),
                    // Let's hide the password with *.
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullNameR = u.FirstName + " " + u.LastName,
                    // Concatenate the first name and last name
                    // with a white space for the full name.
                    RegistrationDate = u.RegistrationDate,
                    BirthDate = u.BirthDate,
                    Gender = u.Gender,
                    IsOnline = u.IsOnline,
                    Score = u.Score,
                    StatusId = u.StatusId
                }).ToList()
            });
            // s: Status entity delegate, u: User entity delegate.

            return Task.FromResult(query);
        }
    }
}
