using APUW.Model.DTOs.Events;
using APUW.Model.DTOs.Tickets;
using APUW.Tests.Util;
using APUW.Tests.Util.ClientExtensions;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Tests.Integration
{
    public class TicketTests(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper)
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task CreateTicket_AsBoardMember_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);
            var response = await _client.CreateTicket("Ticket1", "Content", board.Id);
            var result = await response.GetResult<TicketDto>(outputHelper);

            response.EnsureSuccessStatusCode();
            Assert.NotNull(result?.Data);
            Assert.Equal("Ticket1", result.Data.Name);
        }

        [Fact]
        public async Task CreateTicket_AsNonMember_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (_, user) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);

            await _client.Login(user.Username, user.Password);
            var response = await _client.CreateTicket("Ticket1", "Content", board.Id);
            var result = await response.GetResult<TicketDto>(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task GetTicketList_AsBoardMember_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);
            var response = await _client.GetAsync($"/api/boards/{board.Id}/tickets");
            var result = await response.GetResult<List<TicketDto>>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result?.Data);
        }

        [Fact]
        public async Task GetTicketList_AsNonMember_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (_, user) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);

            await _client.Login(user.Username, user.Password);
            var response = await _client.GetAsync($"/api/boards/{board.Id}/tickets");
            var result = await response.GetResult<List<TicketDto>>(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task GetTicket_AsAdmin_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            var ticket = await _client.CreateTicketAndGetResult(outputHelper, "Ticket1", "Content", board.Id);

            var admin = DataHelpers.GetAdminUserRegisterPayload();
            await _client.Login(admin.Username, admin.Password);
            var response = await _client.GetAsync($"/api/boards/{board.Id}/tickets/{ticket.Id}");
            var result = await response.GetResult<TicketDto>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result?.Data);
        }

        [Fact]
        public async Task UpdateTicket_AsAssignee_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);
            var ticket = await _client.CreateTicketAndGetResult(outputHelper, "Ticket1", "Content", board.Id, userId);

            await _client.Login(user.Username, user.Password);
            var response = await _client.UpdateTicket(board.Id, ticket.Id, "NewName", "NewContent", userId);
            var result = await response.GetResult<TicketDto>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result?.Data);
            Assert.Equal("NewName", result.Data.Name);
            Assert.Equal("NewContent", result.Data.Content);
        }

        [Fact]
        public async Task UpdateTicket_AsNonAssignee_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);
            var ticket = await _client.CreateTicketAndGetResult(outputHelper, "Ticket1", "Content", board.Id);

            await _client.Login(user.Username, user.Password);
            var response = await _client.UpdateTicket(board.Id, ticket.Id, "NewName", "NewContent", userId);
            var result = await response.GetResult<TicketDto>(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task CreateTicket_WithNonBoardMemberAssignee_ReturnsBadRequest()
        {
            await factory.SeedDatabaseAsync();

            var (userId, _) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            var response = await _client.CreateTicket("Ticket1", "Content", board.Id, userId);
            await response.GetResult<TicketDto>(outputHelper);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTicket_AsBoardOwner_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            var (_, ownerUser) = await _client.CreateAndGetUser(outputHelper);
            var (userId, user) = await _client.CreateAndGetUser(outputHelper);

            // Owner
            await _client.Login(ownerUser.Username, ownerUser.Password);
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            // Member
            await _client.Login(user.Username, user.Password);
            var ticket = await _client.CreateTicketAndGetResult(outputHelper, "Ticket1", "Content", board.Id, userId);

            // Owner
            await _client.Login(ownerUser.Username, ownerUser.Password);
            var response = await _client.DeleteAsync($"/api/boards/{board.Id}/tickets/{ticket.Id}");
            await response.GetResult(outputHelper);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteTicket_AsAssignee_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (_, ownerUser) = await _client.CreateAndGetUser(outputHelper);
            var (userId, user) = await _client.CreateAndGetUser(outputHelper);

            // Owner
            await _client.Login(ownerUser.Username, ownerUser.Password);
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            // Member
            await _client.Login(user.Username, user.Password);
            var ticket = await _client.CreateTicketAndGetResult(outputHelper, "Ticket1", "Content", board.Id, userId);
            var response = await _client.DeleteAsync($"/api/boards/{board.Id}/tickets/{ticket.Id}");
            await response.GetResult(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetEvents_AfterAssigneeChange_ReturnsAssigneeChangeEvent()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUser(outputHelper);
            var (newAssignedUserId, newAssignedUser) = await _client.CreateAndGetUser(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetResult("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);
            await _client.AddMemberToBoard(board.Id, newAssignedUserId);
            var ticket = await _client.CreateTicketAndGetResult(outputHelper, "Ticket1", "Content", board.Id, userId);

            await _client.Login(user.Username, user.Password);
            await _client.UpdateTicket(board.Id, ticket.Id, "Ticket1", "Content", newAssignedUserId);

            var response = await _client.GetAsync($"/api/boards/{board.Id}/tickets/{ticket.Id}/events");
            var result = await response.GetResult<List<EventDto>>(outputHelper);

            response.EnsureSuccessStatusCode();
            Assert.NotNull(result?.Data);
            Assert.Contains(result.Data, x => x.Content.Contains(newAssignedUser.Username));
        }
    }
}
