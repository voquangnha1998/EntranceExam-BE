using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntranceExam.Repositories.Entities
{
    [Table("token")]
    public class Token
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(250)]
        public string RefreshToken { get; set; }

        [MaxLength(64)]
        public string ExpiresIn { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }

}
