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
    public class UserUpdateRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from User entity.
        // UserName is required and can be minimum 3 maximum 30 characters.
        [Required, StringLength(30, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required, StringLength(15, MinimumLength = 3)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        // We don't need to get the RegistrationDate value from the client
        // since it will be assigned automatically in UserAddHandler class.

        [Required, Range(0, 5)]
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
        [Required]
        public int? StatusId { get; set; }

        // Required may not be defined if a user may not have a role.
        // For users-roles many to many relationship.
        [Required]
        public List<int> RoleIds { get; set; }
    }

    // UserUpdateHandler inherits the DbService class for User entity type,
    // therefore User entity database operations of the DbService
    // class can be used in this class.
    // UserUpdateHandler also implements the Mediator IRequestHandler
    // interface for UserUpdateRequest and CommandResponse types,
    // therefore IRequestHandler interface's Handle method
    // definition can be implemented for types UserUpdateRequest and
    // CommandResponse in this class, which will be used by
    // the related controller's action.
    public class UserUpdateHandler : DbService<User>,
        IRequestHandler<UserUpdateRequest, CommandResponse>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public UserUpdateHandler(DbContext db) : base(db)
        {
        }

        // Override the DbQuery method of the base DbService class to
        // include the UserRole entities to the query. Therefore
        // during update operation, the related UserRole entities
        // will be deleted first and then new related UserRole entities
        // will be inserted through the RoleIds property of the
        // UserUpdateRequest instance.
        protected override IQueryable<User> DbQuery()
        {
            return base.DbQuery().Include(u => u.UserRoles);
            // u: User entity delegate.
        }

        // Checks whether the user with the same trimmed and
        // case sensitive user name other than the request record
        // exists in the Users database table first. If it doesn't exist,
        // gets the User entity by ID and if found updates the User
        // entity mapped from the UserUpdateRequest instance in the Users
        // database table.
        public async Task<CommandResponse> Handle(UserUpdateRequest request, 
            CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(u => u.Id != request.Id &&
                u.UserName == request.UserName.Trim(), cancellationToken))
                return Error("User with the same user name exists!");
            // u: User entity delegate.
            // Trim method removes white space characters in the
            // beginning and at the end.

            // Get the User entity by ID from the Users database table
            // and check if it is null.
            var userEntity = await DbSingleAsync(request.Id, cancellationToken);
            if (userEntity is null)
                return Error("User not found!");

            // Delete the related UserRole entities first.
            if (request.RoleIds is not null)
                DbRemove(userEntity.UserRoles);

            // Then update the User entity properties from the
            // UserUpdateRequest instance then update the entity in the
            // Users database table (third save default parameter
            // of the DbUpdateAsync method is true therefore changes of the
            // DbSets are commited to the related database tables).
            userEntity.UserName = request.UserName.Trim();
            userEntity.Password = request.Password.Trim();
            userEntity.FirstName = request.FirstName?.Trim();
            // Since request.FirstName may be null, ? is used after
            // meaning that if request.FirstName is null, assign
            // null to the User entity's FirstName property else
            // assign the trimmed value of request.FirstName.
            userEntity.LastName = request.LastName?.Trim();
            // Since request.LastName may be null, ? is used after
            // meaning that if request.LastName is null, assign
            // null to the User entity's LastName property else
            // assign the trimmed value of request.LastName.
            // We don't need to update the RegistrationDate value
            // since it will be assigned automatically in the
            // UserAddHandler class.
            userEntity.BirthDate = request.BirthDate;
            userEntity.Gender = request.Gender;
            userEntity.Score = request.Score ?? 0;
            userEntity.StatusId = request.StatusId ?? 0;
            // ??: Null coalescing operator: If request.StatusId is
            // null assign 0 else assign request.StatusId value to the
            // User entity StatusId property. request.StatusId.Value
            // may also be used since request.StatusId is required. 
            // Same for request.Score.
            userEntity.RoleIds = request.RoleIds;
            await DbUpdateAsync(userEntity, cancellationToken);

            return Success(userEntity.Id, "User updated successfully!");
        }
    }
}
