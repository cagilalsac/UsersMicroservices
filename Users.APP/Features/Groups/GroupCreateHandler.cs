using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    /// <summary>
    /// Represents a request model (DTO: Data Trasfer Object) to create a new Group entity.
    /// Inherits from <see cref="Request"/> and implements <see cref="IRequest{CommandResponse}"/> for MediatR integration.
    /// </summary>
    public class GroupCreateRequest : Request, IRequest<CommandResponse>
    {
        /// <summary>
        /// The title of the group to be created.
        /// This property is required and limited to 100 characters.
        /// Data annotations ensure validation at the model binding level.
        /// </summary>
        [Required, StringLength(100)]
        public string Title { get; set; }

        /*
        Some commonly used data annotation attributes in C#:
        [Required]           // Ensures the property must have a value.
        [StringLength]       // Sets maximum (and optionally minimum) length for strings.
        [Length]             // Sets maximum and minimum length for strings.
        [MinLength]          // Specifies the minimum length for strings or collections.
        [MaxLength]          // Specifies the maximum length for strings or collections.
        [Range]              // Defines the allowed range for numeric values.
        [RegularExpression]  // Validates the property value against a regex pattern.
        [EmailAddress]       // Validates that the property is a valid email address.
        [Phone]              // Validates that the property is a valid phone number.
        [Url]                // Validates that the property is a valid URL.
        [Compare]            // Compares two properties for equality (e.g., password confirmation).
        [DisplayName]        // Sets a friendly name for the property (used in error messages/UI).
        [DataType]           // Specifies the data type (e.g., DateTime) for formatting/UI hints.
        ErrorMessage parameter can be set in all data annotations to show custom validation error messages:
        Example 1: [Required(ErrorMessage = "{0} is required!")] where {0} is the DisplayName (used in MVC) if defined otherwise property name.
        Example 2: [StringLength(100, 3, ErrorMessage = "{0} must be minimum {2} maximum {1} characters!")]
        where {0} is the DisplayName (used in MVC) if defined otherwise property name, {1} is the first parameter which is 100 and
        {2} is the second parameter which is 3.
        */
    }

    /// <summary>
    /// Handles the creation of a new group in the system.
    /// Inherits from <see cref="ServiceBase"/> to utilize common service functionality such as 
    /// culture management and returning success / error command responses.
    /// Implements <see cref="IRequestHandler{GroupCreateRequest, CommandResponse}"/> for MediatR request handling.
    /// </summary>
    public class GroupCreateHandler : ServiceBase, IRequestHandler<GroupCreateRequest, CommandResponse>
    {
        private readonly UsersDb _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCreateHandler"/> class.
        /// </summary>
        /// <param name="db">The database context for accessing group entity data.</param>
        public GroupCreateHandler(UsersDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Handles the creation logic for a new group.
        /// Checks for duplicate group titles (case-insensitive, trimmed), and if none exist, creates and saves a new group.
        /// Returns a <see cref="CommandResponse"/> indicating success or error.
        /// </summary>
        /// <param name="request">The group creation request containing the title.</param>
        /// <param name="cancellationToken">Token for cancelling the async operation.</param>
        /// <returns>A <see cref="CommandResponse"/> with the result of the operation.</returns>
        public async Task<CommandResponse> Handle(GroupCreateRequest request, CancellationToken cancellationToken)
        {
            // Check if any group already exists with the same title (case-insensitive, trimmed) preventing duplicate group names in the database.
            // Way 1:
            //var existingEntity = _db.Groups.SingleOrDefaultAsync(groupEntity => 
            //    groupEntity.Title.ToUpper() == request.Title.ToUpper().Trim(), cancellationToken);
            //if (existingEntity is not null) // if (existingEntity != null) can also be written
            //    return Error("Group with the same title exists!");
            // Way 2: 
            if (await _db.Groups.AnyAsync(groupEntity => groupEntity.Title.ToUpper() == request.Title.ToUpper().Trim(), cancellationToken))
                return Error("Group with the same title exists!");

            // Creates a new Group entity with the provided title (trimmed for consistency).
            var entity = new Group()
            {
                Title = request.Title.Trim() // since request.Title has required data annotation and can't be null, assign request.Title's trimmed value
            };

            // Adds the new group entity to the database context.
            _db.Groups.Add(entity); // _db.Add(entity); can also be written

            // Saves changes to the database asynchronously by using Unit of Work (all changes made to the DbSets will be commited to the database once).
            await _db.SaveChangesAsync(cancellationToken);

            // Returns a success response indicating the group was created with the created group entity's Id value.
            return Success("Group created successfully.", entity.Id);

            // There are also non async synchronous versions of methods such as SingleOrDefault, Any and SaveChanges
            // that can be used without await in non async methods.

            /* Some LINQ methods for querying data (async versions already exists):
            Find: Finds an entity with the given primary key value. Returns null if not found. 
            Uses the database context's cache before querying the database.
            Example: var group = _db.Groups.Find(5);
            
            Single: Returns the only element that matches the specified condition(s).
            Throws an exception if no element or more than one element is found.
            Example: var group = _db.Groups.Single(groupEntity => groupEntity.Id == 5);
            
            SingleOrDefault: Returns the only element that matches the specified condition(s), or null if no such element exists.
            Throws an exception if more than one element is found.
            Example: var group = _db.Groups.SingleOrDefault(groupEntity => groupEntity.Id == 5);
            
            First: Returns the first element that matches the specified condition(s).
            Throws an exception if no element is found.
            Example: var group = _db.Groups.First();
            Example: var group = _db.Groups.First(groupEntity => groupEntity.Id > 5 && groupEntity.Title.StartsWith("Jun");
            
            FirstOrDefault: Returns the first element that matches the specified condition(s), or null if no such element exists.
            Example: var group = _db.Groups.FirstOrDefault();
            Example: var group = _db.Groups.FirstOrDefault(groupEntity => groupEntity.Id < 5 || groupEntity.Title == "Senior");
            
            Last: Returns the last element that matches the specified condition(s).
            Throws an exception if no element is found. Usually requires an OrderBy or OrderByDescending clause.
            Example: var group = _db.Groups.OrderByDescending(groupEntity => groupEntity.Id).Last(); 
            gets the first group from the groups descending ordered by Id.
            Example: var group = _db.Groups.OrderBy(groupEntity => groupEntity.Id).Last();
            gets the last group from the groups ordered by Id.
            
            LastOrDefault: Returns the last element that matches the specified condition(s), or null if no such element exists.
            Usually requires an OrderBy or OrderByDescending clause.
            Example: var group = _db.Groups.OrderBy(groupEntity => groupEntity.Id).LastOrDefault();
            Example: var group = _db.Groups.OrderBy(groupEntity => groupEntity.Id).LastOrDefault(groupEntity.Title.Contains("io"));

            Where: Returns the filtered query that matches the specified condition(s). Tolist, SingleOrDefault or FirstOrDefault 
            methods are invoked to get the filtered data.
            Example: var groups = _db.Groups.Where(groupEntity => groupEntity.Id > 5).ToList();

            Note: SingleOrDefault is generally preferred to get single data.
            Note: These LINQ methods can also be used with collections such as lists and arrays.
            */
        }
    }
}
