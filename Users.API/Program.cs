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



var app = builder.Build();

/// <summary>
/// Maps default endpoints such as health checks and liveness probes.
/// </summary>
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    /// <summary>
    /// Enables Swagger middleware for generating and serving API documentation in the development environment.
    /// </summary>
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

/// <summary>
/// Runs the application and starts listening for incoming HTTP requests.
/// </summary>
app.Run();
