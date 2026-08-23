using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    public class UserAddRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from User entity.
        /*
        ErrorMessage parameter can be set in all data annotations 
        to show custom validation error messages:  
        Example 1: [Required(ErrorMessage = "{0} is required!")] 
        where {0} is the property name which is "UserName".
        Example 2: [StringLength(30, 3, 
        ErrorMessage = "{0} must be minimum {2} maximum {1} characters!")] 
        where {0} is the property name which is "UserName", {1} is the 
        first parameter which is 30 and {2} is the second parameter 
        which is 3.
        */
        // UserName is required and can be minimum 3 maximum 30 characters.
        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(30, MinimumLength = 3,
            ErrorMessage = "{0} must be minimum {2} maximum {1} characters!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(15, MinimumLength = 3,
            ErrorMessage = "{0} must be minimum {2} maximum {1} characters!")]
        public string Password { get; set; }

        [StringLength(50, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        // We don't need to get the RegistrationDate value from the client
        // since it will be assigned automatically in the handler.

        // Minimum value can be 0, maximum value can be 5.
        /*
        The type changed from double to double? to be able to use [Required] 
        attribute. If the type is double, the default value will be 0 and 
        the validation will always be successful even if no value is assigned.
        */
        [Required(ErrorMessage = "{0} is required!")]
        [Range(0, 5, ErrorMessage = "{0} must be between {1} and {2}!")]
        public double? Score { get; set; }

        // We don't need to get the IsOnline value from the client
        // since it will be assigned automatically during token
        // authentication.



        // Modify (1 to many) or add (many to many) the relationship properties.
        /* 
        The type changed from int to int? to be able to use [Required] attribute. 
        If the type is int, the default value will be 0 and the validation will 
        always be successful even if no value is assigned.
        */
        // For users-status one to many relationship.
        [Required(ErrorMessage = "{0} is required!")]
        public int? StatusId { get; set; }

        // Required may not be defined if a user may not have a role.
        // For users-roles many to many relationship.
        [Required(ErrorMessage = "At least one role is required!")]
        public List<int> RoleIds { get; set; }
    }

    // UserAddHandler inherits the DbService class for User entity type,
    // therefore User entity database operations of the DbService
    // class can be used in this class.
    // UserAddHandler also implements the Mediator IRequestHandler
    // interface for UserAddRequest and CommandResponse types,
    // therefore IRequestHandler interface's Handle method
    // definition can be implemented for types UserAddRequest and
    // CommandResponse in this class, which will be used by
    // the related controller's action.
    public class UserAddHandler : DbService<User>, 
        IRequestHandler<UserAddRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public UserAddHandler(DbContext db) : base(db)
        {
        }

        // Checks whether the user with the same trimmed and
        // case sensitive user name exists in the Users database table
        // first. If it doesn't exist, inserts the User entity
        // mapped from the UserAddRequest instance to the Users
        // database table.
        public async Task<CommandResponse> Handle(
            UserAddRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(u =>
                u.UserName == request.UserName.Trim(), cancellationToken
            //&& u.BirthDate == request.BirthDate
            // One or many conditions can be used by && (and), || (or)
            // and ! (not) operators.
            ))
                return Error("User with the same user name exists!");
            // u: User entity delegate.
            // Trim method removes white space characters in the
            // beginning and at the end.

            // Create a new User entity instance from the
            // UserAddRequest instance then insert the entity in the
            // Users database table (third save default parameter
            // of the DbAddAsync method is true therefore changes of the
            // DbSets are commited to the related database tables).
            var userEntity = new User
            {
                UserName = request.UserName.Trim(),
                Password = request.Password.Trim(),
                FirstName = request.FirstName?.Trim(),
                // Since request.FirstName may be null, ? is used after
                // meaning that if request.FirstName is null, assign
                // null to the User entity's FirstName property else
                // assign the trimmed value of request.FirstName.
                LastName = request.LastName?.Trim(),
                // Since request.LastName may be null, ? is used after
                // meaning that if request.LastName is null, assign
                // null to the User entity's LastName property else
                // assign the trimmed value of request.LastName.
                RegistrationDate = DateTime.Now,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                Score = request.Score ?? 0,
                StatusId = request.StatusId ?? 0,
                // ??: Null coalescing operator: If request.StatusId is
                // null assign 0 else assign request.StatusId value to the
                // User entity StatusId property. request.StatusId.Value
                // may also be used since request.StatusId is required. 
                // Same for request.Score.
                RoleIds = request.RoleIds
            };
            await DbAddAsync(userEntity, cancellationToken);

            // Entity's Id value will be updated by the database after insert.
            return Success(userEntity.Id, "User created successfully.");
        }
    }
}
