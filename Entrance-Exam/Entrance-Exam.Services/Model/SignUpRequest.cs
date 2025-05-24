using System.ComponentModel.DataAnnotations;

namespace EntranceExam.Services.Model
{
    public class SignUpRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(20, MinimumLength = 8)]
        public string Password { get; set; }

        [Required, StringLength(32)]
        public string FirstName { get; set; }

        [Required, StringLength(32)]
        public string LastName { get; set; }
    }

}
