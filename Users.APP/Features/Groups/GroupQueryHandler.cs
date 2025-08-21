using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    /// <summary>
    /// Represents a MediatR request for querying groups.
    /// Inherits from <see cref="Request"/> and specifies the expected response type as 
    /// <see cref="IQueryable{GroupQueryResponse}"/> to be presented as a list or a single item in APIs.
    /// </summary>
    public class GroupQueryRequest : Request, IRequest<IQueryable<GroupQueryResponse>>
    {
    }

    /// <summary>
    /// Represents the response model for a group query.
    /// The properties of a model are generally copied from the related entity properties 
    /// which are not navigation properties or which have the columns in the related database table.
    /// Inherits from <see cref="Response"/> to include common identifier properties (Id and Guid).
    /// </summary>
    public class GroupQueryResponse : Response
    {
        /// <summary>
        /// Gets or sets the title of the group.
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// Handles group query requests by retrieving group data from the database and projecting it into group response model.
    /// Inherits from <see cref="ServiceBase"/> to manage culture and use success / error command response helpers.
    /// Implements <see cref="IRequestHandler{GroupQueryRequest, IQueryable{GroupQueryResponse}}"/> for MediatR integration.
    /// </summary>
    public class GroupQueryHandler : ServiceBase, IRequestHandler<GroupQueryRequest, IQueryable<GroupQueryResponse>>
    {
        /// <summary>
        /// Gets the database context for accessing user-related entities and performing data operations.
        /// </summary>
        /// <remarks>
        /// The <see cref="UsersDb"/> context is injected via constructor dependency injection.
        /// It provides access to the application's data layer, including groups and other entities.
        /// </remarks>
        private readonly UsersDb _db; // readonly field that can only be assigned at this line or through the constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupQueryHandler"/> class.
        /// </summary>
        /// <param name="db">
        /// The <see cref="UsersDb"/> context used for database operations.
        /// This is typically provided by the dependency injection container.
        /// </param>
        public GroupQueryHandler(UsersDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Handles the <see cref="GroupQueryRequest"/> by querying the Groups table and projecting each group entity object
        /// into a <see cref="GroupQueryResponse"/> response object.
        /// </summary>
        /// <param name="request">The group query request.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// An <see cref="IQueryable{GroupQueryResponse}"/> representing the projected group data.
        /// The query is not executed until enumerated, in other words methods like ToList or SingleOrDefault methods are invoked,
        /// allowing for further composition or deferred execution.
        /// </returns>
        public Task<IQueryable<GroupQueryResponse>> Handle(GroupQueryRequest request, CancellationToken cancellationToken)
        {
            // Query the Groups DbSet and project each group entity to a GroupQueryResponse response.

            // Way 1: types can be used with variables for declarations
            //IQueryable<GroupQueryResponse> query = Db.Groups.Select(groupEntity => new GroupQueryResponse()
            // Way 2: var can also be used therefore the type of the variable (IQueryable<GroupQueryResponse>)
            // will be known dynamically if an assignment is provided, if no assignment, types must be used
            var query = _db.Groups.Select(groupEntity => new GroupQueryResponse()
            {
                Id = groupEntity.Id,         // Maps the entity's integer ID.
                Guid = groupEntity.Guid,     // Maps the entity's GUID.
                Title = groupEntity.Title    // Maps the entity's title to the response.
            });

            // Return the query as a Task result for MediatR compatibility.
            return Task.FromResult(query);
        }
    }
}
