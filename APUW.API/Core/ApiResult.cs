using APUW.Domain.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace APUW.API.Core
{
    public static class ApiResult
    {
        public static IActionResult GetIActionResult(ResultBase result) => result.StatusCode switch
        {
            ResultStatus.Ok => new OkObjectResult(result),
            ResultStatus.BadRequest => new BadRequestObjectResult(result),
            ResultStatus.Unauthorized => new UnauthorizedObjectResult(result),
            ResultStatus.Forbidden => new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden },
            ResultStatus.NotFound => new NotFoundObjectResult(result),
            ResultStatus.Conflict => new ConflictObjectResult(result),
            ResultStatus.Error => new ObjectResult(result) { StatusCode = 500 },
            _ => new ObjectResult(new Result
            {
                Messages = ["An unexpected error occurred"],
                StatusCode = ResultStatus.Error
            })
            { StatusCode = 500 }
        };
    }
}
