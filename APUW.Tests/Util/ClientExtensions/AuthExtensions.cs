using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Auth;
using APUW.Model.DTOs.Auth.Requests;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Util.ClientExtensions
{
    public static class AuthExtensions
    {
        private static async Task SetBearerToken(this HttpClient client, HttpResponseMessage? response = null)
        {
            if (response == null)
            {
                client.DefaultRequestHeaders.Authorization = null;
                return;
            }

            var loginResult = await response.Content.ReadFromJsonAsync<Result<LoginResultDto>>();
            if (loginResult?.Data?.Token == null) return;

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult.Data.Token);
        }

        public static async Task<HttpResponseMessage> Login(this HttpClient client, string username, string password)
        {
            var loginPayload = new LoginRequestDto
            {
                Username = username,
                Password = password
            };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginPayload);
            await client.SetBearerToken(loginResponse);
            return loginResponse;
        }

        public static async Task<HttpResponseMessage> Register(this HttpClient client, string username, string password)
        {
            var registerPayload = new RegisterRequestDto
            {
                Username = username,
                Password = password,
            };

            var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerPayload);
            return registerResponse;
        }

        public static async Task RegisterAndLogin(this HttpClient client, string username, string password)
        {
            await client.Register(username, password);
            await client.Login(username, password);
        }

        public static async Task RegisterAndLogin(this HttpClient client)
        {
            var randomUser = DataHelpers.GenerateRegisterRequest();
            await client.Register(randomUser.Username, randomUser.Password);
            await client.Login(randomUser.Username, randomUser.Password);
        }

        public static async Task Logout(this HttpClient client)
        {
            await client.SetBearerToken(null);
        }

        public static async Task<(int UserId, RegisterRequestDto Request)> CreateAndGetUser(this HttpClient client, ITestOutputHelper outputHelper)
        {
            var user = DataHelpers.GenerateRegisterRequest();
            await client.Register(user.Username, user.Password);
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
            {
                Password = user.Password,
                Username = user.Username,
            });
            var loginResult = await loginResponse.GetResult<LoginResultDto>(outputHelper);
            Assert.NotNull(loginResult?.Data);
            return (loginResult.Data.Id, user);
        }
    }
}
