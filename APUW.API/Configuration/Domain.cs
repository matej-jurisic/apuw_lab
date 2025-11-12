using APUW.Domain.Interfaces;
using APUW.Domain.Services;

namespace APUW.API.Configuration
{
    public static class Domain
    {
        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IBoardsService, BoardsService>();
            services.AddScoped<ITicketsService, TicketsService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}
