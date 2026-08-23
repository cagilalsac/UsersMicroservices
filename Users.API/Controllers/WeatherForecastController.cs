using Microsoft.AspNetCore.Mvc;

namespace Users.API.Controllers
{
    /// <summary>
    /// API controller for a weather forecast demonstration.
    /// Inherits from the ControllerBase class for using
    /// some of the base methods such as Ok, NoContent, 
    /// BadRequest, etc.
    /// </summary>
    [ApiController] // Attribute declaring WeatherForecastController
                    // is an API controller.
    [Route("[controller]")] // The route of the controller is:
                            // ~/WeatherForecast
    public class WeatherForecastController : ControllerBase
    {
        // Private static field for storing weather statuses array. 
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", 
            "Balmy", "Hot", "Sweltering", "Scorching"
        };

        // Optional private read only field for ILogger instance
        // Dependency Injection through the constructor.
        // Not used in any action but can be used for logging.
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")] // Declares that the action
                                               // is a HTTP GET action.
        // Name property specifies a unique identifier for the specific route
        // (~/WeatherForecast) which allows to generate links to this endpoint
        // from other parts of the application. It behaves as a route tag and
        // does not change the actual public URL path.
        public IEnumerable<WeatherForecast> Get()
        {
            // Returns an array (collection) of random dates, temperatures
            // and statuses through the WeatherForecast response model
            // as JSON (JavaScript Object Notation).
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
