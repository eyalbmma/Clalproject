using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServiceAppointment.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [JsonIgnore] // לא יגיע מהלקוח
        public string CustomerId { get; set; }

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        public string BranchId { get; set; }

        public string Status { get; set; } = "Scheduled";
    }


}
