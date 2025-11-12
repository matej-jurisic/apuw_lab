using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Auth;
using APUW.Model.DTOs.Auth.Requests;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace APUW.Tests.Util
{
    public static class ClientExtensions
    {
        private static async Task SetBearerToken(this HttpClient client, HttpResponseMessage response)
        {
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
            await Register(client, randomUser.Username, randomUser.Password);
            await Login(client, randomUser.Username, randomUser.Password);
        }
    }
}
