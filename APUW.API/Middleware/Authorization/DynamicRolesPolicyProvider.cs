using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace APUW.API.Middleware.Authorization
{
    public class DynamicRolesPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "RoleFromDb:";

        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new DefaultAuthorizationPolicyProvider(options);

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var rolesString = policyName[POLICY_PREFIX.Length..];

                var roles = rolesString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new RolesFromDbRequirement(roles))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
