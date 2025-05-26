using Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [Authorize]
        [HttpDelete("users/{identityNumber}")]
        public async Task<IActionResult> DeleteUserByIdentityNumber(string identityNumber)
        {
            // 1. שליפת המשתמש המחובר מתוך הטוקן
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token");

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return NotFound("Current user not found");

            // 2. בדיקת הרשאות – רק מנהל יכול למחוק
            if (!currentUser.IsAdmin)
                return Forbid("Only admin users can delete users");

            // 3. חיפוש המשתמש למחיקה לפי תעודת זהות
            var userToDelete = await _userManager.Users
                .FirstOrDefaultAsync(u => u.IdentityNumber == identityNumber);

            if (userToDelete == null)
                return NotFound($"No user found with Identity Number {identityNumber}");

            // 4. מחיקת המשתמש
            var result = await _userManager.DeleteAsync(userToDelete);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User deleted successfully" });
        }


        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            // 1. שלוף את ה-Id של המשתמש המחובר מתוך ה-Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token");

            // 2. שלוף את המשתמש מתוך הדאטאבייס
            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return NotFound("User not found");

            // 3. ודא שהוא מנהל
            if (!currentUser.IsAdmin)
                return Forbid("Only admin users can access this resource");

            // 4. שלוף את כל המשתמשים למעט המשתמש שמחובר כרגע
            var users = await _userManager.Users
                .Where(u => u.Id != userId)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.IdentityNumber,
                    u.IsAdmin
                })
                .ToListAsync();

            return Ok(users);
        }






        // פעולה לרישום משתמש חדש

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,  // משתמש במייל כ־UserName
                Email = model.Email,     // מייל של המשתמש
                PhoneNumber = model.Phone,  // טלפון של המשתמש
                FullName = model.FullName,  // שם מלא של המשתמש
                IdentityNumber = model.IdentityNumber,  // מספר זהות של המשתמש
                IsAdmin = model.IsAdmin  // קבלת המידע אם המשתמש הוא מנהל או לא
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User created successfully" });
        }



        // פעולה להתחברות למערכת (Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);

            // החזרת ה-token יחד עם שם המשתמש
            return Ok(new { token, userName = user.NormalizedUserName });
        }


        // יצירת JWT Token
        private string GenerateJwtToken(ApplicationUser user)
        {
            Console.WriteLine("Loaded audience: " + _configuration["Jwt:Audience"]);
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("role", user.IsAdmin ? "admin" : "user")
    };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // 🔐 מפתח פנימי שלך
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Domain"],
                audience: _configuration["Jwt:Audience"], // 🔁 זו השורה הקריטית!
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
