using NotificationService.API.Interfaces;
using NotificationService.API.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NotificationService.API.Services
{
    public class NotificationSenderService : INotificationSenderService
    {
        private readonly ILogger<NotificationSenderService> _logger;

        // הוספת ILogger כדי לתעד את הלוגים
        public NotificationSenderService(ILogger<NotificationSenderService> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(Notification notification)
        {
            try
            {
                if (notification == null)
                {
                    throw new ArgumentNullException(nameof(notification), "Notification cannot be null");
                }

                _logger.LogInformation("Sending notification of type {Type} to {Recipient}", notification.Type, notification.To);

                // הדמיית שליחת התראה (כמובן, פה אנחנו מדמים את השליחה)
                Console.WriteLine($"Sending {notification.Type} to {notification.To}: {notification.Subject} - {notification.Message}");

                _logger.LogInformation("Notification sent successfully to {Recipient} with subject {Subject}", notification.To, notification.Subject);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid notification data provided.");
                throw; // זריקת החריגה כדי ש־Middleware יתפוס אותה
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the notification.");
                throw; // זריקת החריגה כדי ש־Middleware יתפוס אותה
            }
        }
    }
}
