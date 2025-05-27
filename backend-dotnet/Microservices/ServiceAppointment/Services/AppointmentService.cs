using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAppointment.Data;
using ServiceAppointment.Interfaces;
using ServiceAppointment.Models;
using ServiceAppointment.API.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ServiceAppointment.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentDbContext _context;
        private readonly ICacheService _cache;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;  // הוספת RabbitMqPublisher
        private const string CacheKey = "appointments_all";

        // הזרקת כל השירותים
        public AppointmentService(AppointmentDbContext context, ICacheService cache, ILogger<AppointmentService> logger, IRabbitMqPublisher rabbitMqPublisher)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _rabbitMqPublisher = rabbitMqPublisher;  // הזרקת RabbitMqPublisher
        }

        // קבלת כל התורים
        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            var cached = await _cache.GetAsync<IEnumerable<Appointment>>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("✅ Returning appointments from Redis cache");
                return cached;
            }

            _logger.LogInformation("⛔ Cache miss — querying DB");
            var appointments = await _context.Appointments.ToListAsync();
            await _cache.SetAsync(CacheKey, appointments, TimeSpan.FromMinutes(5));
            _logger.LogInformation("✅ Appointments saved to Redis cache");
            return appointments;
        }

        // קבלת תור לפי מזהה
        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _context.Appointments.FindAsync(id);
        }

        // קבלת תורים לפי מזהה לקוח
        public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.Appointments
                .Where(a => a.CustomerId == customerId && a.Status == "Scheduled")
                .ToListAsync();
        }

        // קבלת תורים לפי מזהה סניף
        public async Task<IEnumerable<Appointment>> GetByBranchIdAsync(string branchId)
        {
            return await _context.Appointments
                .Where(a => a.BranchId == branchId && a.Status == "Scheduled")
                .ToListAsync();
        }

        // יצירת תור חדש
        public async Task<Appointment> CreateAsync(Appointment appointment)
        {
            appointment.Status = "Scheduled";
            appointment.ScheduledTime = DateTime.SpecifyKind(appointment.ScheduledTime, DateTimeKind.Utc);

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync(CacheKey); // 💥 מחיקת הקאש כי יש מידע חדש
            _logger.LogInformation("🧹 Cache invalidated after create");

            // שליחת הודעה ל-RabbitMQ אחרי יצירת התור
            var message = new
            {
                AppointmentId = appointment.Id,
                Status = appointment.Status,
                ScheduledTime = appointment.ScheduledTime,
                Customerid = appointment.CustomerId
            };
            _rabbitMqPublisher.PublishNotification(message);

            return appointment;
        }

        // ביטול תור
        public async Task<bool> CancelAsync(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;

            appt.Status = "Cancelled";
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync(CacheKey); // 💥 מחיקת הקאש כי יש שינוי במידע
            _logger.LogInformation("🧹 Cache invalidated after cancel");

            // שליחת הודעה ל-RabbitMQ אחרי ביטול התור
            var message = new
            {
                AppointmentId = appt.Id,
                Status = appt.Status,
                ScheduledTime = appt.ScheduledTime
            };
            _rabbitMqPublisher.PublishNotification(message);

            return true;
        }

        // בדיקת זמינות של תור באותו זמן בסניף
        public async Task<bool> IsSlotTakenAsync(string branchId, DateTime normalizedTime)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.BranchId == branchId &&
                a.ScheduledTime.Year == normalizedTime.Year &&
                a.ScheduledTime.Month == normalizedTime.Month &&
                a.ScheduledTime.Day == normalizedTime.Day &&
                a.ScheduledTime.Hour == normalizedTime.Hour &&
                a.ScheduledTime.Minute == normalizedTime.Minute
            );
        }
    }
}
