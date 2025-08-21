namespace CORE.APP.Models
{
    /// <summary>
    /// Abstract base class for all requests.
    /// </summary>
    public abstract class Request
    {
        /// <summary>
        /// Gets or sets the ID of the request.
        /// </summary>
        public int Id { get; set; }
    }
}
