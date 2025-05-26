using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Authentication.Models;

namespace Authentication.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // הגדרת מודלים
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // הוספת אינדקסים ייחודיים על דוא"ל ומספר זהות
            modelBuilder.Entity<ApplicationUser>()
     .HasIndex(u => u.Email)
     .IsUnique(true);  // שימוש באופציה פשוטה יותר

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.IdentityNumber)
                .IsUnique(true);

        }
    }
}
