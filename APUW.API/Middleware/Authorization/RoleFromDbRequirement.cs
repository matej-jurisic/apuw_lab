using Microsoft.AspNetCore.Authorization;

namespace APUW.API.Middleware.Authorization
{
    public class RolesFromDbRequirement(string[] roles) : IAuthorizationRequirement
    {
        public string[] AllowedRoles { get; } = roles;
    }
}
