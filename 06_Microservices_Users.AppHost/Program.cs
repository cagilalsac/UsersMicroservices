var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Users_API>("users-api");

builder.AddProject<Projects.Locations_API>("locations-api");

builder.Build().Run();
