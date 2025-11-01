# Project Development Roadmap

Note: The development of Locations Microservices will not be explained. You need to develop it as homework while learning from the Users Microservices. 
      You also need to develop another Microservice Project for a different domain such as Products, Movies, Books, etc. including the 
      Users and optionally Locations Microservices. Users and Locations diagrams with some project example diagrams can be found at:  
      https://need4code.com/DotNet?path=.NET%5C00_Files%5CProjects%5CDiagrams.jpg

Note: A simplified version of the Clean Architecture and Repository Pattern that include the basic concepts and structures 
      are applied to the project. More information about Clean Architecture and other architectures can be found at:  
      https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures

Note: Domain is for data access from a database, features are for business logic and API is for presentation. 
      Mediator and CQRS (Command Query Response Segregation) Patterns are also applied to this project.

Note: The source code of the project with search options can also be found at:  
      https://need4code.com/DotNet/Home/Index?path=.NET%5C06_Microservices_Users

Note: We will try to apply the SOLID Principles as much as possible in our project.  
      SOLID Principles:  
      1. Single Responsibility Principle (SRP)  
         A class should have only one reason to change, meaning it should have only one job or responsibility.  
      2. Open/Closed Principle (OCP)  
         Software entities (classes, modules, functions) should be open for extension but closed for modification.  
         You should be able to add new functionality without changing existing code.  
      3. Liskov Substitution Principle (LSP)  
         Subtypes must be substitutable for their base types. Derived classes should extend base classes without changing their behavior.  
      4. Interface Segregation Principle (ISP)  
         No client should be forced to depend on methods it does not use. Prefer small, specific interfaces over large, general-purpose ones.  
      5. Dependency Inversion Principle (DIP)  
         High-level modules should not depend on low-level modules; both should depend on abstractions (e.g., interfaces).  
         This is commonly implemented in ASP.NET Core using dependency injection.

## 1. Environment and Tools

1. Visual Studio Community installation for Windows:  
   https://need4code.com/DotNet/Home/Index?path=.NET%5C00_Files%5CVisual%20Studio%20Community%5CInstallation.pdf

2. Rider for MAC:  
   https://www.jetbrains.com/rider

3. SQLite Database:  
   https://www.sqlite.org

## 2. Solution Setup

4. Create a .NET Aspire Empty App project.

5. Name the solution as your project name. You may change the solution folder in Location. "Create in new folder" option must be checked.

6. (Week 1) Select .NET 8.0 as the Framework, check Configure for HTTPS and select .NET Aspire version as 8.2.  
   
Note: If you get an exception about "ASPIRE_ALLOW_UNSECURED_TRANSPORT" environment variable when you run the Aspire project, open 
launchSettings.json under the Properties folder of the AppHost project and add "ASPIRE_ALLOW_UNSECURED_TRANSPORT": "true" in both 
https and http profiles as in:  
https://github.com/cagilalsac/UsersMicroservices/tree/master/06_Microservices_Users.AppHost/Properties/launchSettings.json

Note: When you run your application in Rider on Mac and if you get the exception below:  
Failed to read NuGet.Config due to unauthorized access. Path: '/Users/YourUserFolder/.nuget/NuGet/NuGet.Config'  
you should run the following commands in the terminal of Rider:  
sudo chown $USER ~/.nuget/NuGet/NuGet.Config  
chmod 644 ~/.nuget/NuGet/NuGet.Config

## 3. CORE Project

7. Right-click the solution in Solution Explorer, then Add -> New Project to create a project named CORE as a Class Library with .NET 8.0.

8. Set Nullable to Disable for all class library projects (via project properties or XML).

9. Create the folders and classes under the CORE project as below:  
   https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Domain/Entity.cs  
   https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Request.cs  
   https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Response.cs  
   https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/CommandResponse.cs  
   https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/ServiceBase.cs

## I) Users Microservices: General topics explained in details in this file and project files.

## 4. Group Entity - Users.APP Project

10. Create a new project under your solution as Class Library (.NET 8) and name it Users.APP.

11. Set Nullable to Disable for Users.APP (via project properties or XML).

12. Right-click Users.APP Project then click Add -> Project Reference then select the CORE Project
    to use the classes of the CORE Project in the Users.APP Project.

13. Create the Group entity class under the Domain folder:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/Group.cs

14. Right-click Users.APP Project then click Manage NuGet Packages then in Browse tab search for System.Data.SQLite.Core latest version and install
    then search for Microsoft.EntityFrameworkCore.Sqlite latest version starting with 8 and install.

15. Create the UsersDb DbContext class under the Domain folder:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/UsersDb.cs

## 4. Group Entity - Users.API Project

16. Create a new project named Users.API with .NET 8, none authentication type, configure for HTTPS, enable OpenAPI support, 
    use controllers and enlist in .NET Aspire orchestration options.

17. Right-click Users.API Project then click Set as Startup Project.
 
18. Right-click Users.API Project then click Manage NuGet Packages then in Browse tab search for Microsoft.EntityFrameworkCore.Tools
    latest version starting with 8 and install.

19. Right-click Users.API Project then click Add -> Project Reference then select the Users.APP Project
    to use both the classes of the CORE Project and Users.APP Project in the Users.API Project.

20. Open appsettings.json and add the ConnectionStrings section:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/appsettings.json
 
21. Open Program.cs and add builder.Services.AddDbContext...  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Program.cs

22. (Week 2) Create your UsersDB database using migrations:  
    - Open Package Manager Console from Visual Studio menu -> Tools -> NuGet Package Manager -> Package Manager Console  
    - Set Users.APP as Default Project in Package Manager Console  
    - Run:  
      add-migration v1  
      update-database  
    - For Rider, you can use the UI as described in JetBrains documentation, or from the terminal  
      Install:  
      dotnet tool install -g dotnet-ef  
      Run:  
      dotnet ef migrations add v1  
      dotnet ef database update -p Users.APP -s Users.API  
    - You can see the created UsersDB database file in Users.API Project.  
    - Optionally in Visual Studio, you can install the SQLite and SQL Server Compact Toolbox extension  
      from Visual Studio menu -> Extensions -> Manage Extensions to connect to the created UsersDB SQLite database.

## 4. Group Entity - Users.APP Project

23. Right-click Users.APP Project then click Manage NuGet Packages then in Browse tab search for MediatR latest version and install.

24. Under Features/Groups, add GroupQueryHandler:  
    The request, response and handler classes can also be in different files such as request class in GroupQueryRequest.cs, 
    response class in GroupQueryResponse.cs and handler class in GroupQueryHandler.cs.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Groups/GroupQueryHandler.cs

    - Handler classes are business logic classes that first get entity data from the database through the database class (UsersDb), 
      which inherits Entity Framework Core's DbContext class for data access, convert the data to the response model object 
      and return the response model object to the controller action for presentation (query).  
      Secondly, handler classes get request model data from the controller action, convert the data to the entity object for create 
      and update operations, or use unique identifier (ID) for delete operation, in the database. Then they return a response model object 
      to the controller action to present the result of the operation (create, update and delete).

    - Request and response model classes are also called Data Transfer Object (DTO) classes.
    
    - Synchronous methods execute tasks one after another. Each operation must complete before the next one starts. The calling thread 
      waits (or "blocks") until the method finishes.  
      Asynchronous methods allow tasks to run in the background. The calling thread does not wait for the operation to finish and 
      can continue executing other code. In C#, asynchronous methods often use the async and await keywords, enabling non-blocking operations 
      (such as I/O or database calls) and improving application responsiveness.

## 4. Group Entity - Users.API Project

25. Add builder.Services.AddMediatR... in Program.cs:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Program.cs

26. (Week 3) Right-click Users.API Controllers folder then Add -> Controller -> Common -> API -> API Controller - Empty to create the GroupsController, 
    implement the Mediator injection and Get actions:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/GroupsController.cs

    - ActionResult inheritance:  
      IActionResult: general return type of actions in a controller  
      |  
      ActionResult: base class that implements IActionResult  
      |  
      OkObjectResult (returned by Ok method) - NotFoundResult (returned by NotFound method) -  
      BadRequestObjectResult (returned by BadRequest method) - etc.
    
## 4. Group Entity - Users.APP Project

27. Under Features/Groups, add GroupCreateHandler:  
    The request and handler classes can also be in different files such as request class in GroupCreateRequest.cs  
    and handler class in GroupCreateHandler.cs.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Groups/GroupCreateHandler.cs
 
    - Attributes gain new features to the fields, properties, methods or classes. When they are used in entities or requests, 
      they are also called data annotations which provide data validations.
    
      Some commonly used data annotation attributes in C#:  
      [Required]           // Ensures the property must have a value.  
    
      [StringLength]       // Sets maximum (and optionally minimum) length for strings.  
    
      [Length]             // Sets maximum and minimum length for strings.  
    
      [MinLength]          // Specifies the minimum length for strings or collections.  
    
      [MaxLength]          // Specifies the maximum length for strings or collections.  
    
      [Range]              // Defines the allowed range for numeric values.  
    
      [RegularExpression]  // Validates the property value against a regex pattern.  
    
      [EmailAddress]       // Validates that the property is a valid email address.  
    
      [Phone]              // Validates that the property is a valid phone number.  
    
      [Url]                // Validates that the property is a valid URL.  
    
      [Compare]            // Compares two properties for equality (e.g., password confirmation).  
    
      [DisplayName]        // Sets a friendly name for the property (used in error messages/UI).  
    
      [DataType]           // Specifies the data type (e.g., DateTime) for formatting/UI hints.  
    
      ErrorMessage parameter can be set in all data annotations to show custom validation error messages:  
      Example 1: [Required(ErrorMessage = "{0} is required!")]  
      where {0} is the DisplayName (used in MVC) if defined otherwise property name.  
    
      Example 2: [StringLength(100, 3, ErrorMessage = "{0} must be minimum {2} maximum {1} characters!")]  
      where {0} is the DisplayName (used in MVC) if defined otherwise property name, {1} is the first parameter which is 100 and 
      {2} is the second parameter which is 3.

    - Some LINQ (Language Integrated Query) methods for querying data (async versions already exists):  
      Find: Finds an entity with the given primary key value. Returns null if not found.  
      Uses the database context's cache before querying the database.  
      Example: var group = _db.Groups.Find(5);  
    
      Single: Returns the only element that matches the specified condition(s).  
      Throws an exception if no element or more than one element is found.  
      Example: var group = _db.Groups.Single(groupEntity => groupEntity.Id == 5);  
    
      SingleOrDefault: Returns the only element that matches the specified condition(s), or null if no such element exists.  
      Throws an exception if more than one element is found.  
      Example: var group = _db.Groups.SingleOrDefault(groupEntity => groupEntity.Id == 5);  
    
      First: Returns the first element that matches the specified condition(s).  
      Throws an exception if no element is found.  
      Example: var group = _db.Groups.First();  
      Example: var group = _db.Groups.First(groupEntity => groupEntity.Id > 5 && groupEntity.Title.StartsWith("Jun");  
    
      FirstOrDefault: Returns the first element that matches the specified condition(s), or null if no such element exists.  
      Example: var group = _db.Groups.FirstOrDefault();  
      Example: var group = _db.Groups.FirstOrDefault(groupEntity => groupEntity.Id < 5 || groupEntity.Title == "Senior");  
    
      Last: Returns the last element that matches the specified condition(s).  
      Throws an exception if no element is found. Usually requires an OrderBy or OrderByDescending clause.  
      Example: var group = _db.Groups.OrderByDescending(groupEntity => groupEntity.Id).Last();  
               gets the first group from the groups descending ordered by Id.  
      Example: var group = _db.Groups.OrderBy(groupEntity => groupEntity.Id).Last();  
               gets the last group from the groups ordered by Id.  
   
      LastOrDefault: Returns the last element that matches the specified condition(s), or null if no such element exists.  
      Usually requires an OrderBy or OrderByDescending clause.  
      Example: var group = _db.Groups.OrderBy(groupEntity => groupEntity.Id).LastOrDefault();  
      Example: var group = _db.Groups.OrderBy(groupEntity => groupEntity.Id).LastOrDefault(groupEntity.Title.Contains("io"));  
   
      Where: Returns the filtered query that matches the specified condition(s). Tolist, SingleOrDefault or FirstOrDefault 
      methods are invoked to get the filtered data.  
      Example: var groups = _db.Groups.Where(groupEntity => groupEntity.Id > 5).ToList();  
    
      Note: SingleOrDefault is generally preferred to get single data.  
      Note: These LINQ methods can also be used with collections such as lists and arrays.

## 4. Group Entity - Users.API Project

28. Add Post method in the GroupsController:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/GroupsController.cs

## 4. Group Entity - Users.APP Project

29. Under Features/Groups, add GroupUpdateHandler:  
    The request and handler classes can also be in different files such as request class in GroupUpdateRequest.cs  
    and handler class in GroupUpdateHandler.cs.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Groups/GroupUpdateHandler.cs

## 4. Group Entity - Users.API Project

30. Add Put method in the GroupsController:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/GroupsController.cs

## 4. Group Entity - Users.APP Project

31. Under Features/Groups, add GroupDeleteHandler:  
    The request and handler classes can also be in different files such as request class in GroupDeleteRequest.cs  
    and handler class in GroupDeleteHandler.cs.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Groups/GroupDeleteHandler.cs

## 4. Group Entity - Users.API Project

32. Add Delete method in the GroupsController:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/GroupsController.cs

## 5. CORE Project - Generic Service

33. Right-click CORE Project then click Manage NuGet Packages then in Browse tab search for Microsoft.EntityFrameworkCore 
    latest version starting with 8 and install.

34. (Week 5) Implement a base abstract generic service class in CORE/APP/Services:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/Service.cs

## 6. Role and User Entities - Users.APP Project

Note: The entities and DbContext class should be implemented first. Second, request, response and handler classes should be implemented. 
      Finally, controllers should be implemented.

35. Create the Role entity class under the Domain folder:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/Role.cs

36. Create the Genders enum under the Domain folder:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/Genders.cs
 
37. Create the User entity class under the Domain folder:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/User.cs

38. Create the UserRole entity class under the Domain folder (for users-roles many to many relationship):  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/UserRole.cs
    
39. Add the Users property in the Group entity class (for group-users one to many relationship):  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/Group.cs

40. Create the Roles, Users and UserRoles DbSets in UsersDb:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/UsersDb.cs

    Note (Optional):  
    Database configurations such as using no action (will not allow to delete the records from the relational table (User) 
    when a record is deleted from the main table (Group)) instead of cascade (which will automatically delete the records 
    from the relational table (User) when a record is deleted from the main table (Group)) between tables may be done by 
    overriding the OnModelCreating method of the UsersDb database context class. Default is cascade in Entity Framework Core. 
    Changing column configurations instead of using data annotations in entities (e.g. making a column required or setting 
    maximum length), changing table names etc. can also be done in the OnModelCreating method among other configurations.

41. Update your UsersDB database for Roles, Users and UserRoles tables using migrations:  
    - Open Package Manager Console from Visual Studio menu -> Tools -> NuGet Package Manager -> Package Manager Console  
    - Right-click Users.API Project and click Set as Startup Project  
    - Set Users.APP as Default Project in Package Manager Console  
    - Run:  
      add-migration v2  
      update-database  
    - For Rider, you can use the UI as described in JetBrains documentation, or from the terminal  
      Run:  
      dotnet ef migrations add v2  
      dotnet ef database update -p Users.APP -s Users.API

42. Under Features/Users, add the following handlers inheriting from the base abstract generic entity Service class:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Users/UserQueryHandler.cs
 
    - Relational entity data can be included to the query by using the Include method (Entity Framework Core Eager Loading). 
      If the included relational entity data has a relation with other entity data, ThenInclude method is used. 
      If you want to automatically include all relational data without using Include / ThenInclude methods (Entity Framework Core Lazy Loading), 
      you need to make the necessary configuration in the class inheriting from DbContext class (UsersDb) to enable Lazy Loading (not recommended).

    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Users/UserCreateHandler.cs  
    
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Users/UserUpdateHandler.cs  

    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Users/UserDeleteHandler.cs

43. Under Features/Roles, add the following handlers:
    
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Roles/RoleQueryHandler.cs  
 
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Roles/RoleCreateHandler.cs  
 
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Roles/RoleUpdateHandler.cs  
 
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Roles/RoleDeleteHandler.cs

44. (Week 6) Since Group entity has relational User entities (group-users one to many relationship),  
    we should check if the group to be deleted has any relational users in the Handle method. If any, we shouldn't delete the group:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Groups/GroupDeleteHandler.cs
 
## 6. Role and User Entities - Users.API Project

45. Download the Scaffolding Templates that will generate the code for the controllers automatically from:  
    https://need4code.com/DotNet/Home/Index?path=.NET%5C00_Files%5CScaffolding%20Templates%5CTemplates.7z  
    Extract the Templates folder in the compressed file to your API Projects. 
    Right-click the Templates folder then click Exclude From Project for the template codes not being compiled and published. 
    If you want to see the excluded folders or files of your project, you can click the fifth icon from left in 
    top of the Solution Explorer with description Show All Files. 
    The excluded files or folders are seen as dashed points in Solution Explorer. 
    If you want to include a folder or file in your project, right-click the file or folder with dashed points 
    then click Include In Project, therefore the codes will be compiled and published. 
    You don't have to use these templates, however if you choose not to, you need to write or modify the dependency injections and actions.

46. Right-click the Controllers folder then Add -> Controller -> Common -> API -> API Controller with actions, using Entity Framework 
    and select Role entity as Model class, select UsersDb as DbContext class and give the name RolesController as Controller name:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/RolesController.cs

    Note: For Rider run:  
    "dotnet aspnet-codegenerator controller -name {ModelName}Controller -m {Namespace}.{ModelName} -dc {Namespace}.{DbContextName} 
    --relativeFolderPath Controllers -api"  
    in the terminal for scaffolding with the templates under the Templates folder.

47. (Week 7) Right-click the Controllers folder then Add -> Controller -> Common -> API -> API Controller with actions, using Entity Framework 
    and select User entity as Model class, select UsersDb as DbContext class and give the name UsersController as Controller name:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/UsersController.cs
    
    Note: Add the action named GetFiltered at the bottom of the UsersController to apply filtering to Users data.
    
    Note: If you want to seed your database with initial test data, you can create a database contoller with a seed action:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/DatabaseController.cs

    Note: If you get any exceptions during scaffolding, create the UsersDbFactory class in Users.APP/Domain folder:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/UsersDbFactory.cs

## 7. JWT (Json Web Token) Authentication and Authorization - CORE Project

48. Right-click the CORE Project then click Manage NuGet Packages then in Browse tab search for Microsoft.AspNetCore.Authentication.JwtBearer 
    latest version starting with 8 and install.

49. Create the TokenRequestBase class in CORE Project's APP/Models/Authentication folder.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Authentication/TokenRequestBase.cs

50. Create the RefreshTokenRequestBase class in CORE Project's APP/Models/Authentication folder.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Authentication/RefreshTokenRequestBase.cs

51. Create the TokenResponse class in CORE Project's APP/Models/Authentication folder.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Authentication/TokenResponse.cs

52. Create the base authentication service class named AuthServiceBase in CORE Project's APP/Services/Authentication folder.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/Authentication/AuthServiceBase.cs

53. Create the token authentication service interface named ITokenAuthService in CORE Project's APP/Services/Authentication folder.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/Authentication/ITokenAuthService.cs

54. Create the token authentication concrete service class named TokenAuthService in CORE Project's APP/Services/Authentication folder.  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/Authentication/TokenAuthService.cs

## 7. JWT (Json Web Token) Authentication and Authorization - Users.APP Project

55. Add RefreshToken and RefreshTokenExpiration properties to the User entity:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Domain/User.cs

56. Update your UsersDB database for the RefreshToken and RefreshTokenExpiration columns of the Users table using migrations:  
    - Open Package Manager Console from Visual Studio menu -> Tools -> NuGet Package Manager -> Package Manager Console  
    - Right-click Users.API Project and click Set as Startup Project  
    - Set Users.APP as Default Project in Package Manager Console  
    - Run:  
      add-migration v3  
      update-database  
    - For Rider, you can use the UI as described in JetBrains documentation, or from the terminal  
      Run:  
      dotnet ef migrations add v3  
      dotnet ef database update -p Users.APP -s Users.API

57. Under Features/Tokens, create TokenHandler:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Tokens/TokenHandler.cs

58. Under Features/Tokens, create RefreshTokenHandler:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Tokens/RefreshTokenHandler.cs

## 7. JWT (Json Web Token) Authentication and Authorization - Users.API Project

59. In Program.cs, add  
    "builder.Services.AddSingleton<ITokenAuthService, TokenAuthService>();"  
    builder.Configuration["SecurityKey"]...  
    builder.Services.AddAuthentication...  
    builder.Services.AddSwaggerGen...  
    app.UseAuthentication...  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Program.cs

    - Service Lifetimes in ASP.NET Core Dependency Injection:  
      I) AddScoped:  
      Lifetime: Scoped to a single HTTP request (or scope).  
      Behavior: Creates one instance of the service per HTTP request.  
      Use case: Use when you want to maintain state or dependencies that last only during a single request.  
      Example: DbContext, which should be shared across operations within a request, generally added with AddDbContext method.
      
      II) AddSingleton:  
      Lifetime: Singleton for the entire application lifetime.  
      Behavior: Creates only one instance of the service for the whole app lifecycle.  
      Use case: Use for stateless services or global shared data/services.  
      Example: Caching services, configuration providers, logging services.
      
      III) AddTransient:  
      Lifetime: Transient (short-lived).  
      Behavior: Creates a new instance every time the service is requested.  
      Use case: Use for lightweight, stateless services that are cheap to create.  
      Example: Utility/helper classes without state.
      
      Notes:  
      Injecting a Scoped service into a Singleton can cause issues due to lifetime mismatch. ASP.NET Core DI container will warn about such mismatches.
    
60. Add Issuer, Audience and TokenMessage sections to appsettings.json:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/appsettings.json

61. Right-click Users.API Controllers folder then Add -> Controller -> Common -> API -> API Controller - Empty to create the TokensController, 
    implement the Mediator and Configuration injections then implement the Token and RefreshToken actions:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/TokensController.cs

62. Add the [Authorize(Roles = "Admin")] attribute for Admin role on top of the RolesController class so that all of the actions can be executed 
    by only authenticated users with role Admin:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/RolesController.cs
 
    - The [Authorize] attribute in ASP.NET Core is used to control access to a controller's actions by requiring that the user is authenticated 
      and, optionally, meets certain authorization requirements.  
    
      When you decorate a controller or action with [Authorize], only authenticated users who provides the Json Web Token or 
      Authentication Cookie (used in MVC Web Applications) can access it. Unauthenticated requests will receive a 401 Unauthorized response.  
    
      You can specify roles or policies to further restrict access. For example, [Authorize(Roles = "Admin")] allows only authenticated users 
      in the "Admin" role.  
    
      You can apply [Authorize] at the controller level (to protect all actions) or at the action level (to protect specific endpoints).  
    
      Example usages:  
      [Authorize]: Any authenticated user can access.  
    
      [Authorize(Roles = "User")]: Only authenticated users with role "User" can access.  

      [Authorize(Roles = "Admin,User")]: Only authenticated users with the "Admin" or "User" role can access.  
    
      [AllowAnonymous]: Can be used to override [Authorize] at the action level and allows public access, therefore gives permission to everyone 
      for executing specific actions.

63. Add the [Authorize] attribute on top of the GroupsController class. For example, if the Get action is wanted to be executed by 
    authorized and unauthorized users (eveyone), [AllowAnonymous] attribute can be defined. The Get by ID action can be executed by only 
    authenticated users since Authorize is defined at controller level. Add [Authorize(Roles = "Admin")] attribute on top of the 
    Post, Put and Delete actions so that only authenticated users with role Admin can execute these actions:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/GroupsController.cs

64. Add the [Authorize(Roles = "Admin,User")] attribute, or [Authorize] attribute since we have only 2 roles, on top of the Get and 
    GetFiltered actions of the UsersController class so that authenticated users with all roles can execute these actions. 
    Then add [Authorize(Roles = "Admin")] attribute on top of the Get by ID, Post, Put and Delete actions so that authenticated users 
    with role Admin can execute these actions:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/UsersController.cs

65. To increase the security of APIs for the Production Environment, Cross-Origin Resource Sharing configuration can be added 
    in the Program.cs of the Users.API Project:  
    builder.Services.AddCors...  
    app.UseCors...  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Program.cs

## II) Locations Microservices: Source code shared in Locations.APP and Locations.API Projects, these projects are homeworks.

Note: Homework includes:  
APP Domain: Country and City entities with LocationsDb database context class  
APP Features: Country and City query, create, update and delete handlers, requests and query responses  
API Controllers: Country and city controllers with get (authorized anonymous), post (authorized for admin role), put (authorized for admin role) 
and delete (authorized for admin role) actions, including GetByCountryId (authorized anonymous) action in cities controller.  

Note (Optional):  
Database configurations such as using no action (will not allow to delete the records from the relational table (City) 
when a record is deleted from the main table (Country)) instead of cascade (which will automatically delete the records 
from the relational table (City) when a record is deleted from the main table (Country)) between tables may be done by 
overriding the OnModelCreating method of the LocationsDb database context class. Default is cascade in Entity Framework Core. 
Changing column configurations instead of using data annotations in entities (e.g. making a column required or setting 
maximum length), changing table names etc. can also be done in the OnModelCreating method among other configurations.

Note: The JWT Authentication will be provided through Users.API, therefore JWT Authentication and validation configuration must be added 
in the IoC Container of the Program.cs in the Locations.API Project with the same security key and validation configuration as in Users.API Project's Program.cs.  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Locations.API/Program.cs  

For reference:  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Program.cs  

Note: Check and add missing injection configurations in the Program.cs of Locations.API for:  
DbContext: builder.Services.AddDbContext...  
Mediator: builder.Services.AddMediatR...  
Authentication: builder.Configuration["SecurityKey"]..., builder.Services.AddAuthentication...  
Swagger: builder.Services.AddSwaggerGen...  
CORS: builder.Services.AddCors...  
Authentication: app.UseAuthentication()  
CORS: app.UseCors()

Note: The GetByCountryId action is added at the bottom of the CitiesController to return a response with cities of a country having the country ID parameter value.  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Locations.API/Controllers/CitiesController.cs

Note: You need to set AppHost Project as the startup project so that you can send requests to the Users.API and Locations.API endpoints through the Aspire Dashboard.

Note: Extra initial data seeding for locations can be found at:  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Locations.API/Controllers/DatabaseController.cs

Note: Extra extensions, inner join and left outer join queries with filtering, paging and ordering example implementations can be found at:  
https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Paging/IPageRequest.cs  

https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Models/Ordering/IOrderRequest.cs  

https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Extensions/StringExtensions.cs  
      
https://github.com/cagilalsac/UsersMicroservices/tree/master/Locations.APP/Features/Locations/LocationInnerJoinQueryHandler.cs  
      
https://github.com/cagilalsac/UsersMicroservices/tree/master/Locations.APP/Features/Locations/LocationLeftJoinQueryHandler.cs  

https://github.com/cagilalsac/UsersMicroservices/tree/master/Locations.API/Controllers/LocationsController.cs

## III) Gateway.API

66.	Create a new project under the solution named Gateway.API, search for web api and select ASP.NET Core Web API template, 
    select .NET 8 as the Framework, Authentication type as None, check Configure for HTTPS, do not check Enable OpenAPI support, 
    do not check Use controllers and check Enlist in .NET Aspire orchestration.

67.	Right click on the Gateway.API project then click Manage NuGet Packages. Under the Browse tab search for Ocelot and install the latest version.

68. Create the ocelot.json file in the Gateway.API project:  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Gateway.API/ocelot.json

    You can get the Host and Port of DownstreamHostAndPorts section from the launchSettings.json files which are under the Properties folder of 
    Users.API and Locations.API. If you are running your solution with https, look for the https profile and use the first url defined at 
    applicationUrl section which are for our solution "localhost:7043" for Users.API, and "localhost:7018" for Locations.API. 
    If you are running your solution with http, look for the http profile and use the url defined at applicationUrl section which are for our solution 
    "localhost:5021" for Users.API, and "localhost:5038" for Locations.API. 
    Now you can reach all of the users and locations endpoints defined in the ocelot.json file through this gateway.

69.	Open the Program.cs file of the Gateway.API and add:  
    "builder.Configuration.AddJsonFile("ocelot.json");"  
    "builder.Services.AddOcelot();"  
    "await app.UseOcelot();"  
    https://github.com/cagilalsac/UsersMicroservices/tree/master/Gateway.API/Program.cs

70.	You can get the url of the Gateway.API from the launchSettings.json file under the Properties folder which is for our solution  
    https: "localhost:7237" (first url defined at applicationUrl of the https profile)  
    http: "localhost:5024" (url defined at applicationUrl of the http profile)  

    You can run the gateway after setting the AppHost Project as the startup project letting you sending requests to Users.API and Locations.API endpoints.  
    You can send a request to the WeatherForecastController of Users.API by writing "test/users" after the localhost with port url 
    in the address bar of your browser. "test/locations" can be written to test the WeatherForecastController of Locations.API.  
 
    Now you can send requests from your client applications or Postman to the gateway. Unfortunaltey, you can't use Swagger to send requests 
    to the gateway with this project configuration, therefore launchSettings.json file in the Properties folder of the Gateway.API Project was modified.

## IV) Extra Consuming Locations Microservices from User Microservices

Create the HttpServiceBase and HttpService classes in the CORE Project's APP/Services/HTTP folder:  
https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/HTTP/HttpServiceBase.cs  
https://github.com/cagilalsac/UsersMicroservices/tree/master/CORE/APP/Services/HTTP/HttpService.cs

Create UserLocationQueryHandler in the Features/Users folder of the Users.APP Project  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.APP/Features/Users/UserLocationQueryHandler.cs

In Program.cs of the Users.API Project, add  
"builder.Services.AddHttpContextAccessor();"  
"builder.Services.AddHttpClient();"  
"builder.Services.AddScoped<HttpServiceBase, HttpService>();"  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Program.cs

Add GetUserLocations action in the UsersController of the Users.API Project  
https://github.com/cagilalsac/UsersMicroservices/tree/master/Users.API/Controllers/UsersController.cs

## V) Extra Client Application consuming Users and Locations Microservices

You may develop a client application with React, Angular, ASP.NET Core MVC, etc. if you want.