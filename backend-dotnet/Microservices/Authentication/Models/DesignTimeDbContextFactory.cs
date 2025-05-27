using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Authentication.Models
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // הגדרת חיבור לדאטה-בייס (החיבור לקובץ appsettings.json)
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=authdb;Username=admin;Password=admin123");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
