using Microsoft.AspNetCore.Identity;

namespace Authentication.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public bool IsAdmin { get; set; }  // הוספת שדה חדש עבור אם המשתמש הוא מנהל
    }

}
