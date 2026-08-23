namespace CORE.Domain
{
    // Base abstract class for all entity, request and response classes.
    public abstract class Record
    {
        // Property for primary key of entities, unique identifier of
        // requests and responses.
        public int Id { get; set; }

        // Constructor with id parameter to set to Id property value
        // from a derived class constructor with id parameter.
        protected Record(int id)
        {
            Id = id;
        }

        // Default constructor to set the Id property default value 0.
        protected Record()
        {
        }
    }
}
