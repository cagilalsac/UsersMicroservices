using CORE.Domain;

namespace CORE.Models
{
    // Concrete class for returning create, update and delete
    // operation results.
    public class CommandResponse : Record
    {
        // Readonly property to return the success status as true or false.
        // Since no setter, the property value can only be set in the below
        // line or in the constructor.
        public bool IsSuccessful { get; }

        // Readonly property to return additional information such as a
        // success message or an error message containing details.
        public string Message { get; }

        // Constructor with parameters to set the IsSuccessful and Message
        // property values with id default parameter value passed to the
        // base abstract Record class constructor with id parameter.
        // If no value is passed for the id parameter, the default value 0
        // will be used.
        public CommandResponse(bool isSuccessful, string message, int id = 0)
            : base(id)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }
    }
}
