using APUW.Domain.Core.Results;
using Microsoft.EntityFrameworkCore;

namespace APUW.Domain.Core.Transactions
{
    public static class DbContextExtensions
    {
        public static async Task<Result<T>> InTransaction<T>(
            this DbContext context,
            Func<Task<Result<T>>> action,
            Func<Result<T>, bool>? shouldRollback = null)
        {
            using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                bool rollback = shouldRollback?.Invoke(result) ?? !result.IsSuccess;

                if (rollback)
                {
                    await tx.RollbackAsync();
                    return result;
                }

                await context.SaveChangesAsync();
                await tx.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Result.Failure(ResultStatus.Error, ex.Message);
            }
        }

        public static async Task<Result> InTransaction(
            this DbContext context,
            Func<Task<Result>> action,
            Func<Result, bool>? shouldRollback = null)
        {
            using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                bool rollback = shouldRollback?.Invoke(result) ?? !result.IsSuccess;

                if (rollback)
                {
                    await tx.RollbackAsync();
                    return result;
                }

                await context.SaveChangesAsync();
                await tx.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Result.Failure(ResultStatus.Error, ex.Message);
            }
        }
    }
}