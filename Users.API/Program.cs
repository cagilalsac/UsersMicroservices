using CORE.APP.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Users.APP.Domain;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Adds default service configurations for the application.
/// This may include health checks, logging, and other shared services.
/// </summary>
builder.AddServiceDefaults();



// -----------------------------------------------------------------------------------
// Add services to the IoC (Inversion of Control) container for Dependency Injections.
// -----------------------------------------------------------------------------------
// Registers the application's DbContext (named 'UsersDb') with the dependency injection container.
// Configures the DbContext to use SQLite as the database provider.
// The connection string named "UsersDb" is retrieved from the application's configuration settings (appsettings.json).
// This setup enables the application to connect to the specified SQLite database when interacting with entity sets.
// Whenever a DbContext injection occurs through the constructor of a class (such as a features class),
// initialize an object of type UsersDb and use this object in the class for database operations.
builder.Services.AddDbContext<DbContext, UsersDb>(options => options.UseSqlite(builder.Configuration.GetConnectionString("UsersDb")));

// Way 1:
// Registers MediatR services with the dependency injection container.
// MediatR is a popular .NET library that implements the mediator pattern, enabling decoupled communication
// between components by sending requests (commands, queries, events) to handlers without direct dependencies.
// The configuration below scans the assembly containing the 'UsersDb' type for any classes that implement
// MediatR handler interfaces (such as IRequestHandler, INotificationHandler, etc.).
// This allows automatic discovery and registration of all MediatR handlers in the specified assembly.
// As a result, you can inject the IMediator interface into controllers and use it to
// send requests or publish notifications, which will be routed to the appropriate handlers.
//builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(UsersDb).Assembly));
// Way 2:
// Iterates through all assemblies currently loaded in the application's AppDomain.
// For each assembly, registers all MediatR handlers (such as IRequestHandler, INotificationHandler, etc.)
// found within that assembly with the dependency injection container.
// This enables MediatR to automatically discover and wire up handlers from any loaded assembly,
// allowing for modular handler organization and dynamic handler loading (e.g., from plugins or feature assemblies).
// Note: Registering handlers from all assemblies can be useful in large or modular applications,
// but may introduce duplicate registrations or performance overhead if not managed carefully.
foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
{
    builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(assembly));
}

// Registers the TokenAuthService as a singleton service for the ITokenAuthService interface.
// Lifetime: Singleton (one instance is created and shared for the entire application lifetime).
// Usage: All dependencies requesting ITokenAuthService will receive the same TokenAuthService instance.
// Rationale: Suitable for stateless services or services that do not depend on per-request data.
// TokenAuthService is stateless and thread-safe, making it appropriate for singleton registration.
// Avoid storing per-user or per-request data in singleton services to prevent concurrency issues.
builder.Services.AddSingleton<ITokenAuthService, TokenAuthService>();

/*
 * Service Lifetimes in ASP.NET Core Dependency Injection:
 *
 * 1. AddScoped:
 *    - Lifetime: Scoped to a single HTTP request (or scope).
 *    - Behavior: Creates one instance of the service per HTTP request.
 *    - Use case: Use when you want to maintain state or dependencies that last only during a single request.
 *    - Example: DbContext, which should be shared across operations within a request.
 *
 * 2. AddSingleton:
 *    - Lifetime: Singleton for the entire application lifetime.
 *    - Behavior: Creates only one instance of the service for the whole app lifecycle.
 *    - Use case: Use for stateless services or global shared data/services.
 *    - Example: Caching services, configuration providers, logging services.
 *
 * 3. AddTransient:
 *    - Lifetime: Transient (short-lived).
 *    - Behavior: Creates a new instance every time the service is requested.
 *    - Use case: Use for lightweight, stateless services that are cheap to create.
 *    - Example: Utility/helper classes without state.
 *
 * Notes:
 * - Injecting a Scoped service into a Singleton can cause issues due to lifetime mismatch.
 * - ASP.NET Core DI container will warn about such mismatches.
 *
 * Summary:
 * | Method        | Lifetime                | Instance Created             | Typical Use Case                  |
 * |---------------|-------------------------|------------------------------|-----------------------------------|
 * | AddScoped     | Per HTTP request        | One instance per request     | DbContext, per-request services   |
 * | AddSingleton  | Application-wide        | One instance for app lifetime| Caching, config, logging          |
 * | AddTransient  | Every time requested    | New instance each time       | Lightweight stateless helpers     |
 */



// --------------
// Authentication
// --------------
// For getting the value for the key SecurityKey in any class injected with IConfiguration instance to be used for JWT.
builder.Configuration["SecurityKey"] = "users_microservices_security_key_2025="; // must be minimum 256 bits
// Enable JWT Bearer authentication as the default scheme.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        // Define rules for validating JWT.
        config.TokenValidationParameters = new TokenValidationParameters
        {
            // Use the builder configuration's security key to create a new symmetric security key for verifying the JWT's signature.
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecurityKey"] ?? string.Empty)),

            ValidIssuer = builder.Configuration["Issuer"], // get Issuer section's value from appsettings.json
            ValidAudience = builder.Configuration["Audience"], // get Audience section's value from appsettings.json

            // These flags ensure the validation of the JWT.
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });



// -----------------------------------------------------------------------------
// HTTP Context to be reached in non-controller classes via Dependency Injection
// -----------------------------------------------------------------------------
// Registers the IHttpContextAccessor service with the dependency injection container.
// This service allows access to the current HttpContext (such as request headers, user identity, etc.)
// from non-controller classes (e.g., services, handlers) via constructor injection of IHttpContextAccessor.
// Useful for scenarios where you need to access HTTP-specific information outside of controllers or middleware.
// Example usage: Retrieving the Authorization header in a MediatR handler to call external APIs on behalf of the user.
builder.Services.AddHttpContextAccessor();



// ------------------------------------
// HTTP Client to consume external APIs
// ------------------------------------
// Registers the IHttpClientFactory service and enables dependency injection for HttpClient instances.
// This allows the application to create and manage HttpClient objects efficiently, supporting features like
// connection pooling, DNS updates, and resilience (e.g., retries, timeouts, and circuit breakers).
// Typical usage: Inject IHttpClientFactory or HttpClient into services, handlers, or controllers to call external APIs.
// Example: Used in UserLocationQueryHandler to fetch country and city data from external microservices.
builder.Services.AddHttpClient();



/// <summary>
/// Adds controller support for handling HTTP API requests.
/// </summary>
builder.Services.AddControllers();

/// <summary>
/// Adds support for API endpoint discovery and OpenAPI/Swagger documentation generation.
/// </summary>
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();



// -------
// Swagger
// -------
// Configure Swagger/OpenAPI documentation, including JWT authentication support in the UI.
builder.Services.AddSwaggerGen(c =>
{
    // Define the basic information for your API.
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1"
    });

    // Add the JWT Bearer scheme to the Swagger UI so JWT can be tested in requests.
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = """
        JWT Authorization header using the Bearer scheme.
        Enter your JWT as: "Bearer jwt"
        Example: "Bearer a1b2c3"
        """
    });

    // Add the security requirement globally so all endpoints are secured unless specified otherwise.
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});



// ---------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) for Production Environment
// ---------------------------------------------------------------
// Registers and configures CORS services for the application.
// CORS is a security feature implemented by browsers to restrict cross-origin HTTP requests
// initiated from scripts running in the browser. By default, web applications are not allowed
// to make requests to a domain different from the one that served the web page.
// The configuration below adds a default CORS policy that allows requests from any origin,
// with any HTTP header, and any HTTP method. This is useful during development or for public APIs,
// but should be restricted in production environments to specific origins for better security.
// Usage:
// - The policy is applied globally if app.UseCors() is called without parameters in the middleware pipeline.
// - To restrict CORS, replace AllowAnyOrigin(), AllowAnyHeader(), and AllowAnyMethod() with more specific rules.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()   // Allows requests from any domain.
        .AllowAnyHeader()   // Allows any HTTP headers in the request.
        .AllowAnyMethod()); // Allows any HTTP method (GET, POST, PUT, DELETE, etc.).
});



var app = builder.Build();

/// <summary>
/// Maps default endpoints such as health checks and liveness probes.
/// </summary>
app.MapDefaultEndpoints();



// Configure the HTTP request pipeline.
// Way 1: Enable Swagger for only the development environment.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
// Way 2: Enable Swagger for both development and production environments.
app.UseSwagger();
app.UseSwaggerUI();

// ASP.NET Core Environments:
// The environment is development when the Users.API application is run from Visual Studio choosing a development profile from the
// launchSettings.json file in the Properties folder of the Users.API Project.
// When the Users.API application is run on a server or from Visual Studio choosing a production profile from the launchSettings.json
// file, the environment is production.
// In launchSettings.json, http profile was defined as Development through ASPNETCORE_ENVIRONMENT section, https profile's
// ASPNETCORE_ENVIRONMENT value was changed to Production from Development for using the production environment.
// The environments can be changed from the drop down list (selected as https) near the run button under the Visual Studio top menu
// when running the application. Before, Users.API must be set as startup project from the drop down list at left of the run button.
// If https (production environment) is selected, sections in appsettings.json will be used for configuration,
// if http (development environment) is selected, sections in appsettings.Development.json will be used for configuration.
// Therefore, same sections with some different values (such as connection string) must present in both files.
// Environment configurations and usage is not a must for the development of applications.



/// <summary>
/// Enforces HTTPS redirection for all HTTP requests.
/// </summary>
app.UseHttpsRedirection();



// --------------
// Authentication
// --------------
// Enable authentication middleware so that [Authorize] works.
app.UseAuthentication();



/// <summary>
/// Adds authorization middleware to the request pipeline.
/// </summary>
app.UseAuthorization();

/// <summary>
/// Maps controller endpoints to handle incoming HTTP requests.
/// </summary>
app.MapControllers();



// ---------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) for Production Environment
// ---------------------------------------------------------------
app.UseCors();



/// <summary>
/// Runs the application and starts listening for incoming HTTP requests.
/// </summary>
app.Run();
