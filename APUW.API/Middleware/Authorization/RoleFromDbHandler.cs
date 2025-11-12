using APUW.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace APUW.API.Middleware.Authorization
{
    public class RoleFromDbHandler(IUsersService usersService) : AuthorizationHandler<RolesFromDbRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesFromDbRequirement requirement)
        {
            var username = context.User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
            {
                context.Fail();
                return;
            }

            var rolesResult = await usersService.GetUserRoles(username);
            if (rolesResult.IsFailure)
            {
                context.Fail();
            }

            var roles = rolesResult.Data;
            if (requirement.AllowedRoles.Any(roles.Contains))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
