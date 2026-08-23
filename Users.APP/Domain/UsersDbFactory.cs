using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Globalization;

namespace Users.APP.Domain
{
    // Provides a factory for creating UsersDb instances at design time.
    // This is used by Entity Framework (EF) tools such as
    // migrations to construct the database context when the
    // application is not running.
    // This class may need to be created if there are any exceptions
    // during Scaffolding.
    public class UsersDbFactory : IDesignTimeDbContextFactory<UsersDb>
    {
        // The connection string in the configuration file
        // (appsettings.json).
        const string CONNECTIONSTRING = "data source=UsersDB.db";


        // Creates a new instance of the UsersDb type using the connection
        // string. This method is called by EF tooling at design time.
        public UsersDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsersDb>();
            optionsBuilder.UseSqlite(CONNECTIONSTRING);
            return new UsersDb(optionsBuilder.Options);
        }



        // Seeds the database tables with initial data (Optional).
        public void SeedDb()
        {
            // Create a new instance of the UsersDb type:
            var db = CreateDbContext(null);

            // Delete existing data:
            var userRoles = db.UserRoles.ToList();
            db.UserRoles.RemoveRange(userRoles);
            var roles = db.Roles.ToList();
            db.Roles.RemoveRange(roles);
            var users = db.Users.ToList();
            db.Users.RemoveRange(users);
            var statuses = db.Statuses.ToList();
            db.Statuses.RemoveRange(statuses);

            // Reset identity columns (for SQLite):
            // IDs will start from 1
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='UserRoles'");
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='Roles'");
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='Users'");
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='Statuses'");

            // Insert new data:
            // Roles:
            var role = new Role() { Name = "Admin" };
            db.Roles.Add(role);
            // Role may also be written instead of Role()
            role = new Role { Name = "User" };
            db.Roles.Add(role);
            db.SaveChanges(); // commit changes to the database
            // Statuses with Users:
            db.Statuses.Add(new Status
            {
                Title = "Active",
                Users = new List<User>
                {
                    new User
                    {
                        UserName = "admin",
                        Password = "admin",
                        RegistrationDate = new DateTime(
                            2026, 7, 16, 20, 57, 45),
                        UserRoles = new List<UserRole>
                        {
                            new UserRole { RoleId = db.Roles.Single(
                                r => r.Name == "Admin").Id }
                        }
                    },
                    new User
                    {
                        UserName = "user",
                        Password = "user",
                        RegistrationDate = DateTime.Parse(
                            "07/16/2026 20:58:13", new CultureInfo("en-US")),
                        UserRoles = new List<UserRole>
                        {
                            new UserRole { RoleId = db.Roles.Single(
                                r => r.Name == "User").Id }
                        }
                    }
                }
            });
            db.Statuses.Add(new Status { Title = "Inactive" });
            db.SaveChanges();
        }
    }
}
