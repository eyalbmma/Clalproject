using NotificationService.API.Models;

namespace NotificationService.API.Interfaces
{
    public interface INotificationSenderService
    {
        Task SendAsync(Notification notification);
    }
}
