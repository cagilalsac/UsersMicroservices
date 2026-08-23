using CORE.Domain;
using System.ComponentModel.DataAnnotations;

namespace Users.APP.Domain
{
    // Status is a Record.
    public class Status : Record
    {
        // Reference Type: string, array, class and interface
        // are reference types (can be null).
        // Title can't be null and must contain maximum 5 characters.
        [Required, StringLength(5)]
        public string Title { get; set; }

        // Reference Type and Navigation Property:
        // Navigation properties are used to navigate from one entity
        // to another related entity to get or set related data.
        // Will be an empty list of User if no assignment
        // or no include to a query.
        // For status-users one to many relationship.
        public List<User> Users { get; set; }
            = new(); // = new List<User>(); can also be written
    }
}
