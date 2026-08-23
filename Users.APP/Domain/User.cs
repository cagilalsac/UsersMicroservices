using CORE.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.APP.Domain
{
    // User is a Record.
    public class User : Record
    {
        // Reference Type: string, array, class and interface
        // are reference types (can be null).
        // UserName can't be null and must contain maximum 30 characters.
        [Required, StringLength(30)]
        public string UserName { get; set; }

        // Reference Type:
        // Password can't be null and must contain maximum 15 characters.
        [Required, StringLength(15)]
        public string Password { get; set; }

        // Reference Type:
        // FirstName can be null and must contain maximum 50 characters.
        // Will be null if no assignment.
        [StringLength(50)]
        public string FirstName { get; set; }


        // Reference Type:
        // LastName can be null and must contain maximum 50 characters.
        // Will be null if no assignment.
        [StringLength(50)]
        public string LastName { get; set; }

        // Value Type: Types other than string, array, class and interface
        // are value types (can't be null).
        // Gender can't be null and will have the first value of the Genders
        // enum if no value is assigned.
        public Genders Gender { get; set; }

        // Value Type: ? after the type is used to make the value type act
        // like reference type.
        // BirthDate can be null since ? is used after the type and
        // will be null if no value is assigned.
        public DateTime? BirthDate { get; set; }

        // Value Type:
        // RegistrationDate can't be null and will have the default value
        // 01/01/0001 00:00:00 (DateTime.MinValue) if no value is assigned.
        public DateTime RegistrationDate { get; set; }

        // Value Type:
        // Score can't be null and will have the default value 0
        // if no value is assigned.
        public double Score { get; set; }

        // Value Type:
        // IsOnline can't be null and will have the default value false
        // if no value is assigned.
        public bool IsOnline { get; set; }

        // Value Type:
        // StatusId can't be null and will have the default value 0
        // if no value is assigned.
        // For users-status one to many relationship.
        public int StatusId { get; set; } // foreign key to the Status entity

        // Reference Type and Navigation Property:
        // Navigation properties are used to navigate from one entity
        // to another related entity to get or set related data.
        // Will be null if no assignment or no include to a query.
        // For users-status one to many relationship.
        public Status Status { get; set; }

        // Reference Type and Navigation Property:
        // Will be an empty list of UserRole if no assignment
        // or no include to a query.
        // For users-roles many to many relationship.
        public List<UserRole> UserRoles { get; set; }
            = new(); // = new List<UserRole>(); can also be written

        // This property helps to easily manage the UserRoles relational entities
        // by User entity's Role Id values.
        // NotMapped attribute means no column in the Users table will be created
        // for this property.
        [NotMapped]
        public List<int> RoleIds
        {
            // Returns the Role Id values of the User entity.
            get => UserRoles.Select(userRoleEntity
                => userRoleEntity.RoleId).ToList();

            // Sets the UserRoles relational entities of the User entity
            // by the assigned Role Id values.
            set => UserRoles = value.Select(roleIdValue => new UserRole
            {
                RoleId = roleIdValue
            }).ToList();
        }



        // The following properties will be used for JWT (JSON Web Token)
        // based authentication.
        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
