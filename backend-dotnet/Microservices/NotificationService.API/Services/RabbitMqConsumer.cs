using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.API.Interfaces;

namespace NotificationService.API.Services
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",//"rabbitmq",
                UserName = "admin",
                Password = "admin123"
            };

            // ניסיון חוזר להתחברות
            const int maxRetries = 5;
            int retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.QueueDeclare(queue: "notifications",
                      durable: true,  // שים לב שהגדרתי את התור כ-durable כדי שלא יימחק
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);

                    Console.WriteLine("✅ Connected to RabbitMQ");
                    break;
                }
                catch (Exception ex)
                {
                    retries++;
                    Console.WriteLine($"❌ RabbitMQ connection failed: {ex.Message}. Retrying in 3s... ({retries}/{maxRetries})");
                    Thread.Sleep(3000);
                }
            }

            if (_connection == null)
            {
                throw new Exception("❌ Failed to connect to RabbitMQ after multiple attempts");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<Notification>(json);

                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<INotificationSenderService>();

                if (notification != null)
                {
                    await service.SendAsync(notification);
                }
            };

            _channel.BasicConsume(queue: "notifications",
                                  autoAck: true,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
