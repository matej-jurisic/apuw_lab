using APUW.Model.DTOs.Auth.Requests;

namespace APUW.Tests.Util
{
    public static class DataHelpers
    {
        public static RegisterRequestDto GenerateRegisterRequest()
        {
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            return new RegisterRequestDto()
            {
                Username = $"test_{uniqueId}",
                Password = "MyStrongPassword123!"
            };
        }

        public static RegisterRequestDto GetAdminUserRegisterPayload()
        {
            return new RegisterRequestDto()
            {
                Username = "admin",
                Password = "Password0"
            };
        }

        public static RegisterRequestDto GetUserRegisterPayload()
        {
            return new RegisterRequestDto()
            {
                Username = "user",
                Password = "Password0"
            };
        }
    }
}
