using APUW.API.Middleware.Authorization;
using APUW.Domain.Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APUW.API.Configuration
{
    public static class Auth
    {
        public static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            string jwtKey = configuration.GetValue<string>("Authentication:JWT:Key") ?? throw new Exception("Missing JWT configuration");
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", opt =>
                {
                    opt.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                    opt.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            var resultObj = Result.Failure(ResultStatus.Unauthorized, "You are not authenticated");
                            var json = System.Text.Json.JsonSerializer.Serialize(resultObj);

                            return context.Response.WriteAsync(json);
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";

                            var resultObj = Result.Failure(ResultStatus.Forbidden);
                            var json = System.Text.Json.JsonSerializer.Serialize(resultObj);

                            return context.Response.WriteAsync(json);
                        }
                    };
                });
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

            services.AddSingleton<IAuthorizationPolicyProvider, DynamicRolesPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, RoleFromDbHandler>();
            services.AddHttpContextAccessor();
        }
    }
}
