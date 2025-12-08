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
        Task<Result<BoardDto>> GetBoard(int boardId);
        Task<Result<BoardDto>> UpdateBoard(int boardId, UpdateBoardRequestDto request);
        Task<Result> DeleteBoard(int boardId);
        Task<Result<List<UserDto>>> GetBoardMembers(int boardId);
        Task<Result<BoardMemberDto>> AddBoardMember(int boardId, int userId);
        Task<Result> RemoveBoardMember(int boardId, int userId);
    }
}
