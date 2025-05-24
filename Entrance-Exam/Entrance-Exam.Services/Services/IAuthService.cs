using EntranceExam.Services.Model;

namespace EntranceExam.Services.Services
{
    public interface IAuthService
    {
        Task<SignUpResponse> SignUpAsync(SignUpRequest request);
        Task<SignInResponse> SignInAsync(SignInRequest request);
        Task<UserInfoDto> GetUserByIdAsync(int userId);
    }
}
