using APUW.API.Configuration;
using APUW.API.Middleware.Exceptions;
using APUW.Model;
using Microsoft.EntityFrameworkCore;

namespace APUW.API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=database.db"));
            builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite("Data Source=database.db"), ServiceLifetime.Scoped);

            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddAuthServices(builder.Configuration);
            builder.Services.AddDomainServices();
            builder.Services.AddSwaggerServices();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();

            if (!app.Environment.IsEnvironment("Testing"))
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}