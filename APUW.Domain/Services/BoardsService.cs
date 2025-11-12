using APUW.Domain.Core.Results;
using APUW.Domain.Core.Transactions;
using APUW.Domain.Interfaces;
using APUW.Model;
using APUW.Model.DTOs.Boards;
using APUW.Model.DTOs.Boards.Requests;
using APUW.Model.DTOs.Users;
using APUW.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace APUW.Domain.Services
{
    public class BoardsService(AppDbContext context, ICurrentUserService currentUserService) : IBoardsService
    {
        private readonly AppDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly UserDto _currentUser = currentUserService.GetCurrentUser();

        public async Task<Result> AddBoardMember(int id, int userId)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");

            var board = await _context.Boards.FindAsync(id);

            if (board == null) return Result.Failure(ResultStatus.NotFound, "Board not found.");

            if (!isAdmin && board.OwnerId != _currentUser.Id)
                return Result.Failure(ResultStatus.Forbidden, "You are not authorized to add board members.");

            var existsBoardMember = await _context.UserBoards.AnyAsync(x => x.BoardId == id && x.UserId == userId);

            if (existsBoardMember) return Result.Failure(ResultStatus.Conflict, "User is already a member of the board.");

            var userBoard = new UserBoard
            {
                UserId = id,
                BoardId = board.Id,
            };

            await _context.UserBoards.AddAsync(userBoard);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<BoardDto>> CreateBoard(CreateBoardRequestDto request)
        {
            var newBoard = new Board
            {
                Name = request.Name,
                OwnerId = _currentUser.Id
            };

            return await _context.InTransaction(async () =>
            {
                await _context.Boards.AddAsync(newBoard);
                await _context.SaveChangesAsync();

                await _context.UserBoards.AddAsync(new UserBoard
                {
                    BoardId = newBoard.Id,
                    UserId = _currentUser.Id
                });
                await _context.SaveChangesAsync();

                return await GetBoard(newBoard.Id);
            });
        }

        public async Task<Result> DeleteBoard(int id)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");

            var board = await _context.Boards.FirstOrDefaultAsync(x => x.Id == id);

            if (board == null) return Result.Failure(ResultStatus.NotFound, "Board not found.");

            if (!isAdmin && board.OwnerId != _currentUser.Id)
                return Result.Failure(ResultStatus.Forbidden, "You are not authorized to delete the board.");

            _context.Boards.Remove(board);
            return Result.Success();
        }

        public async Task<Result<BoardDto>> GetBoard(int id)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");

            var board = await _context.Boards.Select(x => new BoardDto
            {
                Id = x.Id,
                IsOwner = x.OwnerId == _currentUser.Id,
                OwnerUsername = x.Owner.Username,
                Name = x.Name
            }).FirstOrDefaultAsync(x => x.Id == id);

            if (board == null) return Result.Failure(ResultStatus.NotFound, "Board not found.");

            if (isAdmin) return Result.Success(board);

            var isMember = await _context.UserBoards.AnyAsync(x => x.UserId == _currentUser.Id && x.BoardId == id);

            if (!isMember) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to access the board.");

            return Result.Success(board);
        }

        public async Task<Result<List<BoardDto>>> GetBoardList()
        {
            var isAdmin = await _currentUserService.HasRole("Admin");

            if (isAdmin)
            {
                var adminBoardList = await _context.Boards.Select(x => new BoardDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsOwner = x.OwnerId == _currentUser.Id,
                    OwnerUsername = x.Owner.Username
                }).ToListAsync();

                return Result.Success(adminBoardList);
            }

            var boardList = await _context.UserBoards.Where(x => x.UserId == _currentUser.Id).Select(x => new BoardDto
            {
                Id = x.Board.Id,
                Name = x.Board.Name,
                IsOwner = x.Board.OwnerId == _currentUser.Id,
                OwnerUsername = x.Board.Owner.Username,
            }).ToListAsync();

            return Result.Success(boardList);
        }

        public async Task<Result<List<UserDto>>> GetBoardMembers(int id)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(x => x.Id == id);

            if (board == null) return Result.Failure(ResultStatus.NotFound, "Board not found.");

            var isAdmin = await _currentUserService.HasRole("Admin");
            var isMember = await _context.UserBoards.AnyAsync(x => x.UserId == _currentUser.Id && x.BoardId == board.Id);

            if (!isMember && !isAdmin) return Result.Failure(ResultStatus.Unauthorized, "You are not authorized to access the board.");

            var boardMembers = await _context.UserBoards.Where(x => x.BoardId == id).Select(x => new UserDto
            {
                Id = x.UserId,
                Roles = x.User.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Username = x.User.Username
            }).ToListAsync();

            return Result.Success(boardMembers);
        }

        public async Task<Result> RemoveBoardMember(int id, int userId)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");

            var board = await _context.Boards.FindAsync(id);

            if (board == null) return Result.Failure(ResultStatus.NotFound, "Board not found.");

            if (!isAdmin && board.OwnerId != _currentUser.Id && userId != _currentUser.Id)
                return Result.Failure(ResultStatus.Forbidden, "You are not authorized to remove other board members.");

            var userBoard = await _context.UserBoards.FirstOrDefaultAsync(x => x.BoardId == id && x.UserId == userId);
            if (userBoard == null) return Result.Failure(ResultStatus.BadRequest, "User is not a member of the board.");
            _context.UserBoards.Remove(userBoard);

            var userTickets = await _context.Tickets.Where(x => x.AssignedToUserId == userId).ToListAsync();
            foreach (var ut in userTickets) ut.AssignedToUserId = null;

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<BoardDto>> UpdateBoard(int id, UpdateBoardRequestDto request)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(x => x.Id == id);

            if (board == null)
            {
                return Result.Failure(ResultStatus.NotFound, "Board not found");
            }

            if (board.OwnerId != _currentUser.Id)
            {
                return Result.Failure(ResultStatus.Forbidden, "Only owners can update boards.");
            }

            board.Name = request.Name;
            await _context.SaveChangesAsync();

            return await GetBoard(id);
        }
    }
}
