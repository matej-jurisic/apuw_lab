
using APUW.API;
using APUW.Model;
using APUW.Model.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace APUW.Tests.Util
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = "TestDatabase_" + Guid.NewGuid().ToString();
        private Action<IServiceCollection>? _configureTestServices;

        public CustomWebApplicationFactory()
        {
            ClientOptions.BaseAddress = new Uri("http://localhost/api/");
        }

        public void SetConfigureTestServices(Action<IServiceCollection> configure)
        {
            _configureTestServices = configure;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName)
                        .ConfigureWarnings(warnings =>
                           warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
                _configureTestServices?.Invoke(services);
            });
        }

        public async Task SeedDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<AppDbContext>();

            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "User" }
            );
            await context.SaveChangesAsync();

            var adminUser = DataHelpers.GetAdminUserRegisterPayload();
            var user = DataHelpers.GetUserRegisterPayload();

            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword(adminUser.Password);
            var userHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var createdAdminUser = (context.Users.Add(new User
            {
                Username = adminUser.Username,
                PasswordHash = adminPasswordHash,
            })).Entity;

            var createdUser = (context.Users.Add(new User
            {
                Username = user.Username,
                PasswordHash = userHash,
            })).Entity;

            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole
            {
                RoleId = context.Roles.FirstOrDefault(x => x.Name == "Admin")!.Id,
                UserId = createdAdminUser.Id
            });

            context.UserRoles.Add(new UserRole
            {
                RoleId = context.Roles.FirstOrDefault(x => x.Name == "User")!.Id,
                UserId = createdUser.Id
            });

            await context.SaveChangesAsync();

            await context.Database.EnsureCreatedAsync();
        }
    }
}
