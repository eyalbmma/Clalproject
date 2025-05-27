using System.ComponentModel.DataAnnotations;

namespace ServiceAppointment.Models.Requests
{
    public class AppointmentRequest
    {
        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        public string BranchId { get; set; }

        public string Status { get; set; } = "Scheduled";
    }
}
