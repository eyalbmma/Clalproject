using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using ServiceAppointment.API.Interfaces;
using Microsoft.Extensions.Logging;

namespace ServiceAppointment.API.Services
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ILogger<RabbitMqPublisher> _logger;

        // הוספתי את ה-ILogger לקונסטרוקטור בשביל היכולת לוגו
        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;
        }

        public void PublishNotification(object message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "admin",
                Password = "admin123"
            };

            try
            {
                // יצירת חיבור ל-RabbitMQ
                _logger.LogInformation("📤 Trying to connect to RabbitMQ...");
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                _logger.LogInformation("✅ Connected to RabbitMQ");

                // יצירת תור
                _logger.LogInformation("📤 Declaring queue 'notifications'...");
                channel.QueueDeclare(queue: "notifications",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                // שליחה ל-RabbitMQ
                _logger.LogInformation("📤 Attempting to send notification to RabbitMQ...");
                channel.BasicPublish(exchange: "",
                                     routingKey: "notifications",
                                     basicProperties: null,
                                     body: body);

                _logger.LogInformation("✅ Notification sent to RabbitMQ.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error while connecting to RabbitMQ: {ex.Message}");
            }
        }
    }
}
