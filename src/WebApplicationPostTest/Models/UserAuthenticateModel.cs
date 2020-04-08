using System.ComponentModel.DataAnnotations;

namespace WebApplicationPostTest.Models
{
    public class UserAuthenticateModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
