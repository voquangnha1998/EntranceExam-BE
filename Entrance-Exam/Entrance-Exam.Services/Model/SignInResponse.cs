namespace EntranceExam.Services.Model
{
    public class SignInResponse
    {
        public UserInfoDto User { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserInfoDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
    }

}
