using ServiceAppointment.Models;

namespace ServiceAppointment.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAsync();                           // קבלת כל התורים
        Task<IEnumerable<Appointment>> GetByCustomerIdAsync(string customerId);  // קבלת תורים לפי מזהה לקוח
        Task<IEnumerable<Appointment>> GetByBranchIdAsync(string branchId);      // קבלת תורים לפי מזהה סניף
        Task<Appointment> GetByIdAsync(int id);                                 // קבלת תור לפי מזהה
        Task<Appointment> CreateAsync(Appointment appointment);                  // יצירת תור חדש
        Task<bool> CancelAsync(int id);                                          // ביטול תור
        Task<bool> IsSlotTakenAsync(string branchId, DateTime normalizedTime);   // בדיקת זמינות תור בסניף ובזמן נתון
    }
}
