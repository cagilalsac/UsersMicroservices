using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Users.APP.Domain
{
    /// <summary>
    /// Represents a user group within the application domain.
    /// Inherits from <see cref="Entity"/> to provide common entity properties such as Id and Guid.
    /// </summary>
    public class Group : Entity
    {
        /// <summary>
        /// Gets or sets the title of the group.
        /// <para>
        /// This property is required and its length is limited to 100 characters.
        /// </para>
        /// <remarks>
        /// The <see cref="RequiredAttribute"/> ensures that the group name must be provided.
        /// The <see cref="StringLengthAttribute"/> restricts the maximum length to 100 characters.
        /// </remarks>
        /// </summary>
        // Required and StringLength are called attributes and they gain new features to the fields, properties, methods or classes.
        // When they are used in entities or requests, they are also called data annotations which provide data validations.
        [Required]
        [StringLength(100)] 
        public string Title { get; set; }



        // for group-users one to many relationship
        public List<User> Users { get; set; } = new List<User>(); // navigation property for retrieving related User entities data
                                                                  // of the Group entity data in queries,
                                                                  // initialized for preventing null reference exception
    }
}
