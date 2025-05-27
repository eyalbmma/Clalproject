using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class RegisterModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string IdentityNumber { get; set; }

        [Required]
        public string Password { get; set; }



        public bool IsAdmin { get; set; }
    }

}
