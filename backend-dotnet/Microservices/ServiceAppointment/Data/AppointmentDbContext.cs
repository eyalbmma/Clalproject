using Microsoft.EntityFrameworkCore;
using ServiceAppointment.Models;


namespace ServiceAppointment.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }
    }
}
