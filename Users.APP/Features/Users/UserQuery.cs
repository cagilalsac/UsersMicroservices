using CORE.Domain;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Roles;
using Users.APP.Features.Statuses;

namespace Users.APP.Features.Users
{
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    public class UserQueryRequest : Record,
        IRequest<IQueryable<UserQueryResponse>>
    {
    }

    // Response properties are created according to the data to be
    // presented in API responses.
    public class UserQueryResponse : Record
    {
        // Copy all the non navigation properties from User entity.
        public string UserName { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public double Score { get; set; }

        public bool IsOnline { get; set; }

        public int StatusId { get; set; }



        // Add the new properties ending with R declaring Response
        // for custom properties or formatted string value properties.
        public string IsOnlineR { get; set; }

        public string ScoreR { get; set; }

        public string RegistrationDateR { get; set; }

        public string BirthDateR { get; set; }

        public string GenderR { get; set; }

        // FirstName + " " + LastName concatenated value.
        public string FullNameR { get; set; }

        // Password value hidden with * characters.
        public string PasswordR { get; set; }




        // Way 1:
        // Property for the Title value of the
        // related Status entity.
        public string StatusTitleR { get; set; }

        // Way 2:
        // Property for the mapped StatusQueryResponse object
        // from the related Status entity.
        // Either Way 1, Way 2 or both can be used.
        public StatusQueryResponse StatusR { get; set; }



        // Way 1:
        // Property for the concatenated role names
        // of related Role entities.
        public string RoleNamesR { get; set; }

        // Way 2:
        // Property for the RoleQueryResponse objects projected
        // from the related Role entities.
        // Either Way 1, Way 2 or both can be used.
        public List<RoleQueryResponse> RolesR { get; set; }
    }

    // UserQueryHandler inherits the DbService class for User entity type,
    // therefore User entity database operations of the DbService
    // class can be used in this class.
    // UserQueryHandler also implements the Mediator IRequestHandler
    // interface for UserQueryRequest and IQueryable of UserQueryResponse
    // types, therefore IRequestHandler interface's Handle method
    // definition can be implemented for types UserQueryRequest and
    // IQueryable of UserQueryResponse in this class, which will be used by
    // the related controller's action.
    public class UserQueryHandler : DbService<User>,
        IRequestHandler<UserQueryRequest,
            IQueryable<UserQueryResponse>>
    {
        // Constructor that passes the injected DbContext instance
        // to the DbService base class constructor.
        public UserQueryHandler(DbContext db) : base(db)
        {
        }

        // Overrides the base virtual DbQuery method to include
        // Status, UserRole and Role entities data to the query
        // to be used in the Handle method and orders the query
        // by User entity Score property descending first, then
        // the ordered records are ordered descending by User
        // entity RegistrationDate property. Finally the ordered
        // records are ordered ascending by User entity UserName
        // property.
        // The overridden DbQuery method can be used in any method
        // below by invoking the DbQuery method.
        protected override IQueryable<User> DbQuery()
        {
            // Relationships:
            // User <-> Status.
            // User <-> UserRoles <-> Roles.
            return base.DbQuery()
                .Include(u => u.Status) // Include Status entities
                                        // from User entities.
                .Include(u => u.UserRoles) // Include UserRole entities
                                           // from User entities.
                .ThenInclude(ur => ur.Role) // Then include Role entities
                                            // from UserRole entities.
                .OrderByDescending(u => u.Score)
                .ThenByDescending(u => u.RegistrationDate)
                .ThenBy(u => u.UserName);
            // Only one OrderBy method must be called.
            // The other called ordering methods must be ThenBy.
            // u: User entity delegate, ur: UserRole entity delegate.
        }

        // Gets the updated query from the DbQuery method then projects
        // the User entities to user query responses and returns the
        // UserQueryResponse query.
        // The returned query can be executed by invoking ToListAsync,
        // SingleOrDefaultAsync, FirstOrDefaultAsync, AnyAsync, etc.
        // LINQ (Language Integrated Query) methods and then the
        // returned result can be used.
        public Task<IQueryable<UserQueryResponse>> Handle(
            UserQueryRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                DbQuery().Select(u => new UserQueryResponse
            {
                // Map each User entity property to UserQueryResponse property.
                Id = u.Id,
                UserName = u.UserName,
                Password = u.Password,
                FirstName = u.FirstName,
                LastName = u.LastName,
                RegistrationDate = u.RegistrationDate,
                BirthDate = u.BirthDate,
                IsOnline = u.IsOnline,
                Gender = u.Gender,
                Score = u.Score,
                StatusId = u.StatusId,
                // Way 1: Map primitive type data for basic information.
                StatusTitleR = u.Status.Title,
                // Way 2: Map complex type object for more information.
                // Either Way 1, Way 2 or both can be used.
                StatusR = new StatusQueryResponse
                {
                    // Map each Status entity property to
                    // UserQueryResponse's StatusQueryResponse property.
                    Id = u.Status.Id,
                    Title = u.Status.Title
                },
                // Way 1: Map primitive type data for basic information.
                RoleNamesR = string.Join(", ", u.UserRoles
                     .OrderBy(ur => ur.Role.Name)
                     .Select(ur => ur.Role.Name)),
                // Way 2: Map complex type object for more information.
                // Either Way 1, Way 2 or both can be used.
                RolesR = u.UserRoles.OrderBy(ur => ur.Role.Name)
                     .Select(ur => new RoleQueryResponse
                     {
                         // Map each Role entity property to
                         // UserQueryResponse's RoleQueryResponse property
                         // through UserRoles.
                         Id = ur.Role.Id,
                         Name = ur.Role.Name
                     }).ToList(),
                // Hidden password with * character.
                PasswordR = new string('*', u.Password.Length),
                // Concatenated first name and list name with white space character.
                FullNameR = u.FirstName + " " + u.LastName,
                // Formatted date and time in month/day/year hour:minute:second
                // format. No need to use the CultureInfo instance for the second
                // parameter of the ToString method since CultureInfo has been
                // assigned in the base abstract Service class by the Culture
                // property assignment. If Culture property of the base abstract
                // Service class is changed within this class constructor,
                // the changed value will be used by the CultureInfo instance and
                // then ToString method.
                RegistrationDateR = u.RegistrationDate
                     .ToString("MM/dd/yyyy HH:mm.ss"),
                // Ternary operator:
                BirthDateR = u.BirthDate.HasValue ? // if (u.BirthDate != null).
                     u.BirthDate.Value.ToShortDateString() : // Assign u.BirthDate's
                                                             // value since u.BirthDate
                                                             // is nullable.
                     string.Empty, // "" string.
                // ToShortDateString returns the date in month/day/year format.
                IsOnlineR = u.IsOnline ? "Yes" : "No",
                GenderR = u.Gender.ToString(), // Assigns "Man" or "Woman".
                ScoreR = u.Score.ToString("N1") // N: Number format.
                                                // 1: 1 decimal.
                                                // C can also be used for currency.
                                                // No need to define the second
                                                // CultureInfo parameter since Culture
                                                // property has been assigned in the
                                                // base abstract Service class.
            })); // u: User entity delegate, ur: UserRole entity delegate.
        }
    }
}
