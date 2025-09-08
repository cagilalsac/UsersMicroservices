using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    // request properties are created according to the data that will be retrieved from APIs or UIs
    public class UserUpdateRequest : Request, IRequest<CommandResponse> 
    {
        // copy all the non navigation properties from User entity
        [Required, StringLength(30, MinimumLength = 4)]
        public string UserName { get; set; }

        [Required, StringLength(15, MinimumLength = 3)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        //public DateTime RegistrationDate { get; set; } // we don't need this property in the request
                                                         // since we won't update it and get its value from the API or UI

        [Range(0, 5)] // minimum value can be 0, maximum value can be 5
        public decimal Score { get; set; }

        public bool IsActive { get; set; }

        public string Address { get; set; }

        // [Required] // can be defined if each user must have a country
        public int? CountryId { get; set; }

        //[Required] // can be defined if each user must have a city
        public int? CityId { get; set; }

        //[Reqired] // can be defined if each user must have a group
        public int? GroupId { get; set; }

        //[Required] // can be defined if each user must have at least one role
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserUpdateHandler : Service<User>, IRequestHandler<UserUpdateRequest, CommandResponse>
    {
        public UserUpdateHandler(DbContext db) : base(db)
        {
        }

        // Since User entity has relational UserRole entities and the RoleIds of the User entity will be updated in the Handle method,
        // we first need to delete the relational UserRoles in the Handle method.
        // Therefore, we include the UserRoles to the query to get the relational UserRoles data to be deleted.
        protected override IQueryable<User> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).Include(u => u.UserRoles);
        }

        public async Task<CommandResponse> Handle(UserUpdateRequest request, CancellationToken cancellationToken)
        {
            // u: User entity delegate. Check if an active user excluding the current updated user with the same user name exists.
            if (await Query().AnyAsync(u => u.Id != request.Id && u.IsActive && u.UserName == request.UserName, cancellationToken))
                return Error("Active user with the same user name exists!");

            // get the User entity by ID from the Users table
            // isNoTracking is false for being tracked by EF Core to update the entity
            var entity = await Query(false).SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("User not found!");

            // delete the relational UserRole entities data
            Delete(entity.UserRoles); // will remove the relational entities data from the UserRoles DbSet as: _db.UserRoles.RemoveRange(entity.UserRoles)

            // update retrieved User entity's properties with request properties
            entity.UserName = request.UserName;
            entity.Password = request.Password;
            entity.FirstName = request.FirstName?.Trim(); // ? is used because request.FirstName can be null
            entity.LastName = request.LastName?.Trim(); // ? is used because request.LastName can be null
            entity.Gender = request.Gender;
            entity.BirthDate = request.BirthDate;
            entity.Score = request.Score;
            entity.IsActive = request.IsActive;
            entity.Address = request.Address?.Trim(); // ? is used because request.Address can be null
            entity.CountryId = request.CountryId;
            entity.CityId = request.CityId;
            entity.GroupId = request.GroupId;
            entity.RoleIds = request.RoleIds;

            Update(entity); // will update the entity in the Users DbSet and since save default parameter's value is true, will save changes to the database

            return Success("User updated successfully.", entity.Id);
        }
    }
}
