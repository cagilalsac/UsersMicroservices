using CORE.APP.Domain;

namespace Users.APP.Domain
{
    // for users-roles many to many relationship
    public class UserRole : Entity
    {
        public int UserId { get; set; } // foreign key that references to the Users table's Id primary key

        public User User { get; set; } // navigation property for retrieving related User entity data of the UserRole entity data in queries

        public int RoleId { get; set; } // foreign key that references to the Roles table's Id primary key

        public Role Role { get; set; } // navigation property for retrieving related Role entity data of the UserRole entity data in queries
    }
}
