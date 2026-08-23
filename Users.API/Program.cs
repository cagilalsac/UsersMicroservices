using CORE.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Users.APP.Domain;

// Create a builder for the Web API and initialize
// configuration, logging, etc.
var builder = WebApplication.CreateBuilder(args);



// --------------------------------------------------------
// Add services to the IoC (Inversion of Control) container
// for Dependency Injections.
// --------------------------------------------------------
// Register the application's DbContext dependency injection
// by sending UsersDb instances to the injected service class
// constructors' DbContext or UsersDb parameter, using SQLite
// with the connection string from appsettings.json.
builder.Services.AddDbContext<DbContext, UsersDb>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString(nameof(UsersDb))));
// nameof(UsersDb) = "UsersDb" which is the class name.

// Way 1:
// Registers MediatR services with the dependency injection
// container.
// MediatR is a popular .NET library that implements the
// mediator pattern, enabling decoupled communication
// between components by sending requests (commands, queries,
// events) to handlers without direct dependencies.
// The configuration below scans the assembly containing the
// 'UsersDb' type for any classes that implement MediatR handler
// interfaces (such as IRequestHandler, INotificationHandler,
// etc.).
// This allows automatic discovery and registration of all
// MediatR handlers in the specified assembly.
// As a result, you can inject the IMediator interface into
// controllers and use it to send requests or publish notifications,
// which will be routed to the appropriate handlers.
//builder.Services.AddMediatR(
//  config => config.RegisterServicesFromAssembly(
//      typeof(UsersDb).Assembly));
// Way 2:
// Iterates through all assemblies currently loaded in the
// application's app domain.
// For each assembly, registers all MediatR handlers (such as
// IRequestHandler, INotificationHandler, etc.) found within
// that assembly with the dependency injection container.
// This enables MediatR to automatically discover and wire up
// handlers from any loaded assembly, allowing for modular handler
// organization and dynamic handler loading (e.g., from plugins
// or feature assemblies).
// Note: Registering handlers from all assemblies can be useful
// in large or modular applications, but may introduce duplicate
// registrations or performance overhead if not managed carefully.
foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
{
    builder.Services.AddMediatR(
        config => config.RegisterServicesFromAssemblies(assembly));
}



// --------------
// Authentication
// --------------
// Registers the JwtAuthService as a scoped service for the
// IJwtAuthService interface.
// Lifetime: Scoped (a new instance is created for each
// HTTP request).
// Usage: All dependencies requesting IJwtAuthService will
// receive the same JwtAuthService instance.
builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();



// --------------
// Authentication
// --------------
// For getting the value for the key SecurityKey from
// appsettings.json in any class injected with IConfiguration
// instance to be used for JWT.
builder.Configuration["SecurityKey"] = 
    "users_microservices_security_key_2026="; 
    // must be minimum 256 bits
// Enable JWT Bearer authentication as the default scheme.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        // Define rules for validating JWT.
        config.TokenValidationParameters = new TokenValidationParameters
        {
            // Use the builder configuration's security key to create
            // a new symmetric security key for verifying the JWT's signature.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["SecurityKey"] ?? string.Empty)),

            ValidIssuer = builder.Configuration["Issuer"], 
            // get Issuer section's value from appsettings.json
            ValidAudience = builder.Configuration["Audience"], 
            // get Audience section's value from appsettings.json

            // These flags ensure the validation of the JWT.
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });



// --------------------------
// Authentication and Swagger
// --------------------------
// Configure Swagger/OpenAPI documentation, including JWT authentication support
// in the UI.
builder.Services.AddSwaggerGen(c =>
{
    // Define the basic information for your API.
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users API",
        Version = "v1"
    });

    // Add the JWT Bearer scheme to the Swagger UI so JWT can be tested in requests.
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, 
        new OpenApiSecurityScheme
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

    // Add the security requirement globally so all endpoints are secured unless
    // specified otherwise.
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



// Add support for controllers.
builder.Services.AddControllers();

// Add Swagger support.
// Learn more about configuring Swagger/OpenAPI
// at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// ---------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) for Production Environment
// ---------------------------------------------------------------
// Registers and configures CORS services for the application.
// CORS is a security feature implemented by browsers to restrict
// cross-origin HTTP requests initiated from scripts running in the browser.
// By default, web applications are not allowed to make requests to a domain
// different from the one that served the web page.
// The configuration below adds a default CORS policy that allows requests
// from any origin, with any HTTP header, and any HTTP method.
// This is useful during development or for public APIs, but should be
// restricted in production environments to specific origins for better security.
// Usage:
// - The policy is applied globally if app.UseCors() is called without parameters
// in the middleware pipeline.
// - To restrict CORS, replace AllowAnyOrigin(), AllowAnyHeader(), and
// AllowAnyMethod() with more specific rules.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()   // Allows requests from any domain.
        .AllowAnyHeader()   // Allows any HTTP headers in the request.
        .AllowAnyMethod()); // Allows any HTTP method (GET, POST, PUT, DELETE, etc.).
});



// Build the application.
var app = builder.Build();



// --------------------------
// Authentication and Swagger
// --------------------------
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



// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();



// --------------
// Authentication
// --------------
// Enable authentication middleware so that [Authorize] works.
app.UseAuthentication();



// Enable authorization middleware.
app.UseAuthorization();

// Maps controller endpoints to handle incoming HTTP requests.
app.MapControllers();



// ---------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) for Production Environment
// ---------------------------------------------------------------
app.UseCors();



// Runs the application and starts listening for incoming HTTP requests.
app.Run();
