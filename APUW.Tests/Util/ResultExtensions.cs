using APUW.Domain.Core.Results;
using System.Text.Json;
using Xunit.Abstractions;

namespace APUW.Tests.Util
{
    public static class ResultExtensions
    {
        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<Result<T>?> GetResult<T>(this HttpResponseMessage response, ITestOutputHelper? outputHelper = null)
        {
            return await DeserializeAndLog<Result<T>>(response, outputHelper);
        }

        public static async Task<Result?> GetResult(this HttpResponseMessage response, ITestOutputHelper? outputHelper = null)
        {
            return await DeserializeAndLog<Result>(response, outputHelper);
        }

        private static async Task<T?> DeserializeAndLog<T>(HttpResponseMessage response, ITestOutputHelper? outputHelper)
            where T : class
        {
            var contentString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contentString))
            {
                outputHelper?.WriteLine("Response content is empty.");
                return null;
            }

            T? result = null;
            try
            {
                result = JsonSerializer.Deserialize<T>(contentString, options);
                result.Log(outputHelper);
            }
            catch (JsonException ex)
            {
                outputHelper?.WriteLine($"Failed to deserialize JSON: {ex.Message}");
            }

            return result;
        }

        public static void Log<T>(this T? result, ITestOutputHelper? outputHelper)
        {
            if (outputHelper != null && result != null)
            {
                var messagesProperty = typeof(T).GetProperty("Messages");
                if (messagesProperty != null)
                {
                    if (messagesProperty.GetValue(result) is IEnumerable<string> messages)
                    {
                        foreach (var message in messages)
                        {
                            outputHelper.WriteLine($"Result Message: {message}");
                        }
                    }
                }
            }
        }
    }
}
