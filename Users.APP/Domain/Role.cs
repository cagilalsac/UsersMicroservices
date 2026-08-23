using CORE.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.APP.Domain
{
    // Role is a Record.
    public class Role : Record
    {
        // Reference Type: string, array, class and interface
        // are reference types (can be null).
        // Required and StringLength are called attributes
        // (or data annotations when used in an entity or a request).
        // Name can't be null and must contain maximum 25 characters.
        [Required, StringLength(25)]
        public string Name { get; set; } // = "Admin";
                                         // Initial assignments may also
                                         // be done to the properties.
                                         // No need to assign "Admin" here.

        // Reference Type and Navigation Property:
        // Navigation properties are used to navigate from one entity
        // to another related entity to get or set related data.
        // Will be an empty list of UserRole if no assignment
        // or no include to a query.
        // For roles-users many to many relationship.
        public List<UserRole> UserRoles { get; set; }
            = new(); // = new List<UserRole>(); can also be written

        // This property helps to easily manage the UserRoles relational entities
        // by Role entity's User Id values.
        // NotMapped attribute means no column in the Roles table will be created
        // for this property.
        //[NotMapped]
        //public List<int> UserIds
        //{
        //    // Returns the User Id values of the Role entity.
        //    get => UserRoles.Select(userRoleEntity
        //        => userRoleEntity.UserId).ToList();
        //
        //    // Sets the UserRoles relational entities of the Role entity
        //    // by the assigned User Id values.
        //    set => UserRoles = value.Select(userIdValue => new UserRole
        //    {
        //        UserId = userIdValue
        //    }).ToList();
        //}
        // Since we won't update the relational user data (UserRoles)
        // through Role entity, we don't need the UserIds property here.
    }
}
