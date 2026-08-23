using CORE.Models;
using System.Globalization;

namespace CORE.Services
{
    // Abstract base class for culture configuration and returning
    // successful or failure operation result CommandResponse objects.
    public abstract class Service
    {
        // Field to store and retrieve the backing culture value
        // using Encapsulation with the Culture property.
        private string _culture;

        // Property to retrieve the culture value and set the culture value
        // with assigning the service's current thread's culture information
        // to "en-US" for United States English or "tr-TR" for Turkish to
        // ensure consistent formatting and localization.
        protected string Culture
        {
            get
            {
                return _culture;
            }
            set
            {
                _culture = value;
                Thread.CurrentThread.CurrentCulture = new CultureInfo(_culture);
                Thread.CurrentThread.CurrentUICulture =
                    new CultureInfo(_culture);
            }
        }

        // Default constructor to set the default culture to "en-US" for
        // United States English. "tr-TR" can be assigned to the Culture property
        // in the derived class constructor or methods for Turkish culture.
        protected Service()
        {
            Culture = "en-US";
        }

        // Behavior (method, function) to return a successful CommandResponse
        // object for the operation with the entity's ID and an optional message
        // default parameter.
        // If no message is provided, a default success message is used
        // by using the Null Coalescing Operator meaning that if message is null,
        // return "Operation successful." message, otherwise return
        // the provided message.
        protected CommandResponse Success(int id, string message = default)
            => new CommandResponse(true, message ?? "Operation successful.", id);

        // Behavior to return a failure CommandResponse object for the operation
        // with an optional message default parameter. If no message is provided,
        // a default error message is used by using the Null Coalescing Operator
        // meaning that if message is null, return "Operation failed!" message,
        // otherwise return the provided message.
        protected CommandResponse Error(string message = default)
            => new CommandResponse(false, message ?? "Operation failed!");
    }
}
