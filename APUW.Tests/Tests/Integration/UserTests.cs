using APUW.Model.DTOs.Users;
using APUW.Model.DTOs.Users.Requests;
using APUW.Tests.Util;
using APUW.Tests.Util.ClientExtensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Tests.Integration
{
    public class UserTests(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper)
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetUserList_AsAuthenticatedUser_ReturnsAllUsers()
        {
            await factory.SeedDatabaseAsync();
            var request = DataHelpers.GetAdminUserRegisterPayload();
            await _client.Login(request.Username, request.Password);

            var response = await _client.GetAsync("/api/users");
            var result = await response.GetResult<List<UserDto>>(outputHelper);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result?.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task ChangeUserRole_AsNonAdmin_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();
            await _client.RegisterAndLogin();
            var response = await _client.PutAsJsonAsync("/api/users/doesnt_matter/roles", new ChangeUserRoleRequestDto
            {
                RoleName = "Admin"
            });
            await response.GetResult(outputHelper);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ChangeUserRole_AsAdmin_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            var (userId, _) = await _client.CreateAndGetUser(outputHelper);

            var adminUser = DataHelpers.GetAdminUserRegisterPayload();
            await _client.Login(adminUser.Username, adminUser.Password);

            var response = await _client.PutAsJsonAsync($"/api/users/{userId}/roles", new ChangeUserRoleRequestDto
            {
                RoleName = "Admin"
            });

            await response.GetResult(outputHelper);
            response.EnsureSuccessStatusCode();
        }
    }
}