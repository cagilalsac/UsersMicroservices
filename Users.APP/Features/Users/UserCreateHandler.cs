using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    // request properties are created according to the data that will be retrieved from APIs or UIs
    public class UserCreateRequest : Request, IRequest<CommandResponse> 
    {
        // copy all the non navigation properties from User entity
        [Required, StringLength(30, MinimumLength = 4)] // user name is required and can be minimum 4 maximum 30 characters
        public string UserName { get; set; }

        [Required, StringLength(15, MinimumLength = 3)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        //public DateTime RegistrationDate { get; set; } // we don't need this property in the request since we won't get its value from the API or UI

        [Range(0, 5)] // minimum value can be 0, maximum value can be 5
        public decimal Score { get; set; }

        public bool IsActive { get; set; }

        public string Address { get; set; }

        public int? CountryId { get; set; }

        public int? CityId { get; set; }
        
        public int? GroupId { get; set; }

        //[Required] // can be defined if each user must have at least one role
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserCreateHandler : Service<User>, IRequestHandler<UserCreateRequest, CommandResponse>
    {
        public UserCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(UserCreateRequest request, CancellationToken cancellationToken)
        {
            // u: User entity delegate. Check if an active user with the same user name exists.
            if (await Query().AnyAsync(u => u.IsActive && u.UserName == request.UserName, cancellationToken))
                return Error("Active user with the same user name exists!");

            var entity = new User
            {
                UserName = request.UserName,
                Password = request.Password,
                FirstName = request.FirstName?.Trim(), // ? is used because request.FirstName can be null
                LastName = request.LastName?.Trim(), // ? is used because request.LastName can be null
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                RegistrationDate = DateTime.Now, // set registration date to the current date and time 
                Score = request.Score,
                IsActive = request.IsActive,
                Address = request.Address?.Trim(), // ? is used because request.Address can be null
                CountryId = request.CountryId,
                CityId = request.CityId,
                GroupId = request.GroupId,
                RoleIds = request.RoleIds
            };

            Create(entity); // will add the entity to the Users DbSet and since save default parameter's value is true, will save changes to the database

            return Success("User created successfully.", entity.Id);
        }
    }
}
