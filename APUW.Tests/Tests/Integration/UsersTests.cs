using APUW.Model.DTOs.Users;
using APUW.Model.DTOs.Users.Requests;
using APUW.Tests.Util;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Tests.Integration
{
    public class UsersTests(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper)
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetAllUsers_AsAdmin_ReturnsList()
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
        public async Task GetAllUsers_AsUser_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();
            await _client.RegisterAndLogin();

            var response = await _client.GetAsync("/api/users");
            var result = await response.GetResult<List<UserDto>>(outputHelper);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task ChangeUserRole_AsUser_ReturnsForbidden()
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
        public async Task ChangeUserRole_AsAdmin_ReturnsOk()
        {
            await factory.SeedDatabaseAsync();

            var newUser = DataHelpers.GenerateRegisterRequest();
            await _client.Register(newUser.Username, newUser.Password);

            var adminUser = DataHelpers.GetAdminUserRegisterPayload();
            await _client.Login(adminUser.Username, adminUser.Password);

            var userList = await (await _client.GetAsync("/api/users")).GetResult<List<UserDto>>(outputHelper);
            var createdUser = userList?.Data.FirstOrDefault(x => x.Username == newUser.Username);
            Assert.NotNull(createdUser);

            var response = await _client.PutAsJsonAsync($"/api/users/{createdUser.Id}/roles", new ChangeUserRoleRequestDto
            {
                RoleName = "Admin"
            });

            await response.GetResult(outputHelper);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}