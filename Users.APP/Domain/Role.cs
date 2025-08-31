using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.APP.Domain
{
    public class Role : Entity
    {
        [Required, StringLength(25)]
        public string Name { get; set; }



        // for users-roles many to many relationship
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>(); // navigation property for retrieving related UserRole
                                                                              // entities data of the Role entity data in queries,
                                                                              // initialized for preventing null reference exception

        // Since we won't update the relational user data (UserRoles) through Role entity, we don't need the UserIds property here.
        //[NotMapped] // no column in the Roles table will be created for this property since NotMapped attribute is defined
        //public List<int> UserIds // helps to easily manage the UserRoles relational entities by User Id values
        //{
        //    // returns the User Id values of the Role entity
        //    get => UserRoles.Select(userRoleEntity => userRoleEntity.UserId).ToList();

        //    // sets the UserRoles relational entities of the Role entity by the assigned User Id values
        //    set => UserRoles = value.Select(userId => new UserRole() { UserId = userId }).ToList(); 
        //}
    }
}
