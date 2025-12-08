using APUW.Model.DTOs.Tickets;
using APUW.Model.DTOs.Tickets.Requests;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Util.ClientExtensions
{
    public static class TicketExtensions
    {
        public static async Task<HttpResponseMessage> CreateTicket(this HttpClient client, string name, string content, int boardId, int? assigneeId = null)
        {
            return await client.PostAsJsonAsync($"/api/boards/{boardId}/tickets", new CreateTicketRequestDto
            {
                Name = name,
                Content = content,
                AssignedToUserId = assigneeId
            });
        }

        public static async Task<TicketDto> CreateTicketAndGetResult(this HttpClient client, ITestOutputHelper outputHelper, string name, string content, int boardId, int? assigneeId = null)
        {
            var response = await client.CreateTicket(name, content, boardId, assigneeId);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.GetResult<TicketDto>(outputHelper);
            Assert.NotNull(result?.Data);
            return result.Data;
        }

        public static async Task<HttpResponseMessage> UpdateTicket(this HttpClient client, int boardId, int ticketId, string name, string content, int? assigneeId = null)
        {
            return await client.PutAsJsonAsync($"/api/boards/{boardId}/tickets/{ticketId}", new UpdateTicketRequestDto
            {
                AssignedToUserId = assigneeId,
                Name = name,
                Content = content
            });
        }
    }
}
