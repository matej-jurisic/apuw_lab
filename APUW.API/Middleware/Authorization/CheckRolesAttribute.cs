using Microsoft.AspNetCore.Authorization;

namespace APUW.API.Middleware.Authorization
{
    public class CheckRolesAttribute : AuthorizeAttribute
    {
        private const string POLICY_PREFIX = "RoleFromDb:";

        public CheckRolesAttribute(params string[] roles)
        {
            Policy = POLICY_PREFIX + string.Join(",", roles);
        }
    }
}
