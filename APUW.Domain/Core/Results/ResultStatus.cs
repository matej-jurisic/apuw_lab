using System.Text.Json.Serialization;

namespace APUW.Domain.Core.Results
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResultStatus
    {
        Ok,
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        Error,
        NoContent,
        Created
    }

}
