namespace NotificationService.API.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // לדוגמה: Email / SMS
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string CustomerId { get; set; } = string.Empty;  // שדה חדש המכיל את מזהה הלקוח
    }

}
