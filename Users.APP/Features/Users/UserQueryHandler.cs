using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserQueryRequest : Request, IRequest<IQueryable<UserQueryResponse>>
    {
        // Properties used for filtering User entity query through the request:
        // All value type properties are defined as nullable because if they have values, their values will be applied for filtering.
        // Generally range filtering, including start and end values, is applied for DateTime and numeric properties.
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Genders? Gender { get; set; }
        public DateTime? BirthDateStart { get; set; }
        public DateTime? BirthDateEnd { get; set; }
        public decimal? ScoreStart { get; set; }
        public decimal? ScoreEnd { get; set; }
        public bool? IsActive { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? GroupId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }



    // response properties are created according to the data to be presented in API responses or UIs
    public class UserQueryResponse : Response 
    {
        // copy all the non navigation properties from User entity
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Genders Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public decimal Score { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? GroupId { get; set; } 
        public List<int> RoleIds { get; set; }

        // add the new properties, some ending with F for the properties with the same name, for custom or formatted string values
        public string FullName { get; set; }
        public string GenderF { get; set; }
        public string BirthDateF { get; set; }
        public string RegistrationDateF { get; set; }
        public string ScoreF { get; set; }
        public string IsActiveF { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Group { get; set; }
        public List<string> Roles { get; set; }
    }



    // Inherit from the generic entity service class therefore DbContext injected constructor can be automatically created
    // and entity CRUD (create, read, update, delete) methods in the base class can be invoked.
    public class UserQueryHandler : Service<User>, IRequestHandler<UserQueryRequest, IQueryable<UserQueryResponse>>
    {
        public UserQueryHandler(DbContext db) : base(db)
        {
            // if the culture of the application is needed to be changed for this handler, below assignment can be made:
            //CultureInfo = new CultureInfo("tr-TR"); default culture is defined as "en-US" in the base service class
        }

        // base virtual Query method is overriden therefore the overriden query can be used in all other methods
        protected override IQueryable<User> Query(bool isNoTracking = true)
        {
            // u: User entity delegate, ur: UserRole entity delegate
            return base.Query(isNoTracking) // will return Users DbSet
                .Include(u => u.Group) // will include the relational Group data
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role) // will first include the relational UserRoles then Role data
                .OrderByDescending(u => u.IsActive) // query will be ordered descending by IsActive values
                .ThenBy(u => u.RegistrationDate) // after order is done for IsActive, ordered query will be ordered ascending by RegistrationDate values
                .ThenBy(u => u.UserName); // after order is done for RegistrationDate, ordered query will be ordered ascending by UserName values

            // Include, ThenInclude, OrderBy, OrderByDescending, ThenBy and ThenByDescending methods can also be used with DbSets.

            /*
            Relational entity data (Group, UserRoles) can be included to the query by using the Include method (Entity Framework Core Eager Loading).
            If the included relational entity data (UserRoles) has a relation with other entity data (Role), ThenInclude method is used.
            If you want to automatically include all relational data without using Include / ThenInclude methods (Entity Framework Core Lazy Loading), 
            you need to make the necessary configuration in the class inheriting from DbContext class (UsersDb) to enable Lazy Loading (not recommended).
            */
        }

        public Task<IQueryable<UserQueryResponse>> Handle(UserQueryRequest request, CancellationToken cancellationToken)
        {
            // will get the query of all User entities
            var entityQuery = Query();



            // filtering according to the request properties: u is User entity delegate

            // if UserName != null and UserName.Trim() != ""
            if (!string.IsNullOrWhiteSpace(request.UserName))
                // apply user name filtering to the query for exact match
                // Way 1:
                //query = query.Where(u => u.UserName.Equals(request.UserName));
                // Way 2:
                entityQuery = entityQuery.Where(u => u.UserName == request.UserName);

            // if FirstName has a value other than null or empty string without whitespace characters
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                // apply first name filtering to the query for case-sensitive partial match,
                // for partial match StartsWith or EndsWith methods can also be used instead of Contains method
                entityQuery = entityQuery.Where(u => u.FirstName.Contains(request.FirstName.Trim()));

            // if LastName has a value other than null or empty string without whitespace characters
            if (!string.IsNullOrWhiteSpace(request.LastName))
                // apply last name filtering to the query for case-sensitive partial match
                entityQuery = entityQuery.Where(u => u.LastName.Contains(request.LastName.Trim()));

            // if Gender has a value therefore is not null
            if (request.Gender.HasValue) // if (request.Gender is not null) or if (request.Gender != null) can also be written
                // apply gender filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.Gender == request.Gender.Value);

            // if BirthDateStart has a value
            if (request.BirthDateStart.HasValue)
                // apply birth date start filtering to the query for greater than or equal match
                // Way 1: filtering with date and time value (e.g. 08/22/1990 13:45:57)
                //query = query.Where(u => u.BirthDate.HasValue && u.BirthDate.Value >= request.BirthDateStart.Value);
                // Way 2: filtering with date value only (e.g. 08/22/1990)
                entityQuery = entityQuery.Where(u => u.BirthDate.HasValue && u.BirthDate.Value.Date >= request.BirthDateStart.Value.Date);

            // if BirthDateEnd has a value
            if (request.BirthDateEnd.HasValue)
                // apply birth date end without time filtering to the query for less than or equal match
                entityQuery = entityQuery.Where(u => u.BirthDate.HasValue && u.BirthDate.Value.Date <= request.BirthDateEnd.Value.Date);

            // if ScoreStart has a value
            if (request.ScoreStart.HasValue)
                // apply score start filtering to the query for greater than or equal match
                entityQuery = entityQuery.Where(u => u.Score >= request.ScoreStart.Value);

            // if ScoreEnd has a value
            if (request.ScoreEnd.HasValue)
                // apply score end filtering to the query for less than or equal match
                entityQuery = entityQuery.Where(u => u.Score <= request.ScoreEnd.Value);

            // if IsActive has a value
            if (request.IsActive.HasValue)
                // apply is active filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.IsActive == request.IsActive.Value);

            // if CountryId has a value
            if (request.CountryId.HasValue)
                // apply country ID filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.CountryId == request.CountryId.Value);

            // if CityId has a value
            if (request.CityId.HasValue)
                // apply city ID filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.CityId == request.CityId.Value);

            // if GroupId has a value
            if (request.GroupId.HasValue)
                // apply group ID filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.GroupId == request.GroupId.Value);

            // if RoleIds has a list with at least one element
            if (request.RoleIds.Count > 0) // Any() method can also be used instead of Count > 0
                // apply role IDs filtering to the query for any match
                entityQuery = entityQuery.Where(u => u.UserRoles.Any(ur => request.RoleIds.Contains(ur.RoleId)));



            // apply entity to response projection to the entity query:
            var query = entityQuery.Select(u => new UserQueryResponse // () after the class name may not be used
            {
                // assigning entity properties to the response
                Id = u.Id,
                Guid = u.Guid,
                UserName = u.UserName,
                Password = u.Password,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Gender = u.Gender,
                BirthDate = u.BirthDate,
                RegistrationDate = u.RegistrationDate,
                Score = u.Score,
                IsActive = u.IsActive,
                Address = u.Address,
                CountryId = u.CountryId,
                CityId = u.CityId,
                GroupId = u.GroupId,
                RoleIds = u.RoleIds,

                // assigning custom or formatted properties to the response
                FullName = u.FirstName + " " + u.LastName,

                GenderF = u.Gender.ToString(), // will assign Woman or Man

                // If User entity's BirthDate value is not null, convert and assign the value with month/day/year format, otherwise assign "".
                // No need to give the second CultureInfo parameter (e.g. new CultureInfo("tr-TR")) to the ToString method since
                // CultureInfo property was assigned in the constructor of the base or this class.
                // Instead of ToString method, ToShortDateString (e.g. 08/18/2025) or ToLongDateString (e.g. Monday, August 18, 2025) methods can be used.
                // For time ToShortTimeString (17:26) or ToLongTimeString (17:26:52) can be used.
                // Again CultureInfo parameter is not needed for these methods.
                BirthDateF = u.BirthDate.HasValue ? u.BirthDate.Value.ToString("MM/dd/yyyy") : string.Empty,

                RegistrationDateF = u.RegistrationDate.ToShortDateString(),
                ScoreF = u.Score.ToString("N1"), // N: number format, C: currency format, 1: one decimal
                IsActiveF = u.IsActive ? "Active" : "Inactive",

                // Way 1: Ternary Operator
                //Country = (u.CountryId.HasValue ? u.CountryId.Value : 0).ToString(),
                // Way 2:
                Country = (u.CountryId ?? 0).ToString(), // If u.CountryId value is null use 0 otherwise use u.CountryId value.

                City = (u.CityId ?? 0).ToString(),
                    
                Group = u.Group != null ? u.Group.Title : null, // Assign the relational Group's Title value if u.Group is not null, otherwise assign null.

                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList() // Get role name values from the relational Role of each UserRole (ur)
                                                                        // and convert them to List<string>.
            });



            // return the query as a Task result
            return Task.FromResult(query);
        }
    }
}
