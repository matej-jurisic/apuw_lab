using APUW.Model.DTOs.Auth;
using APUW.Tests.Util;
using APUW.Tests.Util.ClientExtensions;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Tests.Integration
{
    public class AuthTests(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            await factory.SeedDatabaseAsync();

            var request = DataHelpers.GetUserRegisterPayload();
            var response = await _client.Login(request.Username, request.Password);
            var result = await response.GetResult<LoginResultDto>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result?.Data?.Token);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            await factory.SeedDatabaseAsync();

            var request = DataHelpers.GetUserRegisterPayload();
            request.Password = request.Password + "_wrong";

            var response = await _client.Login(request.Username, request.Password);
            await response.GetResult(outputHelper);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ReturnsConflict()
        {
            await factory.SeedDatabaseAsync();

            var request = DataHelpers.GenerateRegisterRequest();
            await _client.RegisterAndLogin(request.Username, request.Password);

            var response = await _client.Register(request.Username, request.Password);
            await response.GetResult(outputHelper);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMissingConfiguration_ReturnsInternalServerError()
        {
            var factoryWithMissingConfig = new CustomWebApplicationFactory()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        configBuilder.Sources.Clear();
                    });
                });

            var client = factoryWithMissingConfig.CreateClient();

            var request = DataHelpers.GetUserRegisterPayload();
            var registerResponse = await client.Register(request.Username, request.Password);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var response = await client.Login(request.Username, request.Password);
            var result = await response.GetResult(outputHelper);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.NotNull(result?.Messages);
        }
    }
}
