using CORE.Domain;

namespace Users.APP.Domain
{
    // UserRole is a Record.
    // For users-roles many to many relationship.
    public class UserRole : Record
    {
        // Value Type: Types other than string, array, class and interface
        // are value types (can't be null).
        // UserId can't be null and will have the default value 0
        // if no value is assigned.
        public int UserId { get; set; } // foreign key to the User entity

        // Reference Type and Navigation Property: string, array, class
        // and interface are reference types (can be null).
        // Navigation properties are used to navigate from one entity
        // to another related entity to get or set related data.
        // Will be null if no assignment or no include to a query.
        public User User { get; set; }

        // Value Type:
        // RoleId can't be null and will have the default value 0
        // if no value is assigned.
        public int RoleId { get; set; } // foreign key to the Role entity

        // Reference Type and Navigation Property:
        // Will be null if no assignment or no include to a query.
        public Role Role { get; set; }
    }
}
