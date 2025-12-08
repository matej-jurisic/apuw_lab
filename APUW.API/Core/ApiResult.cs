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
            ResultStatus.Created => new ObjectResult(result) { StatusCode = StatusCodes.Status201Created },
            ResultStatus.Forbidden => new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden },
            ResultStatus.NoContent => new NoContentResult(),
            ResultStatus.NotFound => new NotFoundObjectResult(result),
            ResultStatus.Conflict => new ConflictObjectResult(result),
            ResultStatus.Error => new ObjectResult(result) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => new ObjectResult(new Result
            {
                Messages = ["An unexpected error occurred"],
                StatusCode = ResultStatus.Error
            })
            { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }
}
