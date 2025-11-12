using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Boards;
using APUW.Model.DTOs.Boards.Requests;
using APUW.Model.DTOs.Users;

namespace APUW.Domain.Interfaces
{
    public interface IBoardsService
    {
        Task<Result<List<BoardDto>>> GetBoardList();
        Task<Result<BoardDto>> CreateBoard(CreateBoardRequestDto request);
        Task<Result<BoardDto>> GetBoard(int id);
        Task<Result<BoardDto>> UpdateBoard(int id, UpdateBoardRequestDto request);
        Task<Result> DeleteBoard(int id);
        Task<Result<List<UserDto>>> GetBoardMembers(int id);
        Task<Result> AddBoardMember(int id, int userId);
        Task<Result> RemoveBoardMember(int id, int userId);
    }
}
