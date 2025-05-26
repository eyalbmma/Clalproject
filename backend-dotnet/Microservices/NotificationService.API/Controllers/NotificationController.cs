using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Interfaces;
using NotificationService.API.Models;
using Microsoft.Extensions.Logging;  // הוספת ה־using עבור ILogger

namespace NotificationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationSenderService _notificationSenderService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationSenderService notificationSenderService, ILogger<NotificationController> logger)
        {
            _notificationSenderService = notificationSenderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] Notification notification)
        {
            try
            {
                // לוג של קבלת בקשה
                _logger.LogInformation("Received request to send notification to {Recipient}", notification.To);

                if (notification == null)
                {
                    _logger.LogError("Notification data is null.");
                    return BadRequest("Notification data cannot be null.");
                }

                // שליחה לשירות
                await _notificationSenderService.SendAsync(notification);

                // לוג של הצלחה
                _logger.LogInformation("Notification sent successfully to {Recipient}", notification.To);

                return Ok("Notification sent.");
            }
            catch (ArgumentException ex)
            {
                // לוג של שגיאה
                _logger.LogError(ex, "Invalid notification data.");
                return BadRequest("Invalid notification data.");
            }
            catch (Exception ex)
            {
                // לוג של שגיאה כללית
                _logger.LogError(ex, "An error occurred while sending the notification.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
