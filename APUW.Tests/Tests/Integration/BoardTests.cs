using APUW.Model.DTOs.Boards;
using APUW.Model.DTOs.Boards.Requests;
using APUW.Tests.Util;
using APUW.Tests.Util.ClientExtensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace APUW.Tests.Tests.Integration
{
    public class BoardTests(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper)
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task CreateBoard_AsUser_ReturnsSuccessAndUserBecomesOwner()
        {
            await factory.SeedDatabaseAsync();
            var (_, user) = await _client.CreateAndGetUserId(outputHelper);

            await _client.Login(user.Username, user.Password);

            var boardResponse = await _client.CreateBoard("Board1");
            var boardResult = await boardResponse.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, boardResponse.StatusCode);
            Assert.NotNull(boardResult?.Data);
            Assert.Equal(user.Username, boardResult.Data.OwnerUsername);
            Assert.True(boardResult.Data.IsOwner);
        }

        [Fact]
        public async Task CreateBoard_AsAdmin_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();
            var admin = DataHelpers.GetAdminUserRegisterPayload();
            await _client.Login(admin.Username, admin.Password);

            var boardResponse = await _client.CreateBoard("Board1");
            var boardResult = await boardResponse.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, boardResponse.StatusCode);
        }

        [Fact]
        public async Task GetBoardList_AsAuthenticatedUser_ReturnsVisibleBoards()
        {
            await factory.SeedDatabaseAsync();
            await _client.RegisterAndLogin();

            var board1 = await _client.CreateBoard("Board1");
            Assert.Equal(HttpStatusCode.OK, board1.StatusCode);

            await _client.RegisterAndLogin();

            var board2 = await _client.CreateBoard("Board2");
            Assert.Equal(HttpStatusCode.OK, board2.StatusCode);

            var response = await _client.GetAsync("/api/boards");
            var result = await response.GetResult<List<BoardDto>>(outputHelper);

            Assert.NotNull(result?.Data);
            Assert.Single(result.Data);
            Assert.Equal("Board2", result.Data[0].Name);
        }

        [Fact]
        public async Task GetBoard_AsBoardMember_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUserId(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);
            var response = await _client.GetAsync($"/api/boards/{board.Id}");
            var result = await response.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result?.Data);
            Assert.Equal(board.Name, result.Data.Name);
        }

        [Fact]
        public async Task GetBoard_AsNonMember_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (_, user) = await _client.CreateAndGetUserId(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);

            await _client.Login(user.Username, user.Password);
            var response = await _client.GetAsync($"/api/boards/{board.Id}");
            var result = await response.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task UpdateBoard_AsOwner_ReturnsSuccees()
        {
            await factory.SeedDatabaseAsync();
            await _client.RegisterAndLogin();

            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);

            var updateResponse = await _client.PutAsJsonAsync($"/api/boards/{board.Id}", new UpdateBoardRequestDto
            {
                Name = "Board2"
            });
            var updateResult = await updateResponse.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.NotNull(updateResult?.Data);
            Assert.Equal("Board2", updateResult.Data.Name);
        }

        [Fact]
        public async Task UpdateBoard_AsNonOwner_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUserId(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);
            var response = await _client.PutAsJsonAsync($"/api/boards/{board.Id}", new UpdateBoardRequestDto
            {
                Name = "Board2"
            });
            var result = await response.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task DeleteBoard_AsOwner_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();
            await _client.RegisterAndLogin();

            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);

            var deleteResponse = await _client.DeleteAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var response = await _client.GetAsync("/api/boards");
            var result = await response.GetResult<List<BoardDto>>(outputHelper);

            Assert.NotNull(result?.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task DeleteBoard_AsAdmin_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();
            await _client.RegisterAndLogin();

            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);

            var admin = DataHelpers.GetAdminUserRegisterPayload();
            await _client.Login(admin.Username, admin.Password);
            var deleteResponse = await _client.DeleteAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var response = await _client.GetAsync($"/api/boards/{board.Id}");
            var result = await response.GetResult<BoardDto>(outputHelper);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task DeleteBoard_AsNonOwner_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            var (userId, user) = await _client.CreateAndGetUserId(outputHelper);

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);
            var deleteResponse = await _client.DeleteAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

            var response = await _client.GetAsync("/api/boards");
            var result = await response.GetResult<List<BoardDto>>(outputHelper);

            Assert.NotNull(result?.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetBoardMemberList_AsOwner_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);

            var boardMembersResponse = await _client.GetAsync($"/api/boards/{board.Id}/members");

            boardMembersResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetBoardMemberList_AsNonMember_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board1", outputHelper);

            await _client.RegisterAndLogin();

            var boardMembersResponse = await _client.GetAsync($"/api/boards/{board.Id}/members");

            Assert.Equal(HttpStatusCode.Forbidden, boardMembersResponse.StatusCode);
        }

        [Fact]
        public async Task AddBoardMember_AsOwner_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board", outputHelper);

            var (userId, _) = await _client.CreateAndGetUserId(outputHelper);

            var response = await _client.AddMemberToBoard(board.Id, userId);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task AddBoardMember_AsNonOwner_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board", outputHelper);

            var (_, user) = await _client.CreateAndGetUserId(outputHelper);
            await _client.Login(user.Username, user.Password);

            var response = await _client.AddMemberToBoard(board.Id, 100);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RemoveBoardMember_AsSelf_ReturnsSuccess()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board", outputHelper);

            var (userId, user) = await _client.CreateAndGetUserId(outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);

            var response = await _client.RemoveMemberFromBoard(board.Id, userId);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RemoveBoardMember_AsNonOwner_ReturnsForbidden()
        {
            await factory.SeedDatabaseAsync();

            await _client.RegisterAndLogin();
            var board = await _client.CreateBoardAndGetDto("Board", outputHelper);

            var (userId, user) = await _client.CreateAndGetUserId(outputHelper);
            await _client.AddMemberToBoard(board.Id, userId);

            await _client.Login(user.Username, user.Password);

            var response = await _client.RemoveMemberFromBoard(board.Id, 100);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
