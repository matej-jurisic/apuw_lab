using APUW.Model.DTOs.Boards;
using APUW.Model.DTOs.Boards.Requests;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Util.ClientExtensions
{
    public static class BoardExtensions
    {
        public static async Task<HttpResponseMessage> CreateBoard(this HttpClient client, string name)
        {
            return await client.PostAsJsonAsync("/api/boards", new CreateBoardRequestDto
            {
                Name = name
            });
        }

        public static async Task<BoardDto> CreateBoardAndGetResult(this HttpClient client, string name, ITestOutputHelper outputHelper)
        {
            var response = await client.CreateBoard(name);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.GetResult<BoardDto>(outputHelper);
            Assert.NotNull(result?.Data);
            return result.Data;
        }

        public static async Task<HttpResponseMessage> AddMemberToBoard(this HttpClient client, int boardId, int userId)
        {
            var response = await client.PutAsync($"/api/boards/{boardId}/members/{userId}", null);
            return response;
        }

        public static async Task<HttpResponseMessage> RemoveMemberFromBoard(this HttpClient client, int boardId, int userId)
        {
            var response = await client.DeleteAsync($"/api/boards/{boardId}/members/{userId}");
            return response;
        }
    }
}
