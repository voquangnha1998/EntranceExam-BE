using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntranceExam.Repositories.Entities
{
    [Table("user")]
    public class User
    {
        public int Id { get; set; }

        [MaxLength(32)]
        public string FirstName { get; set; }

        [MaxLength(32)]
        public string LastName { get; set; }

        [MaxLength(64)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string Hash { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Token> Tokens { get; set; }
    }

}
