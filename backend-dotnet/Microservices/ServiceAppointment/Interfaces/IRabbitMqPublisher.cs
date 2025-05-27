namespace ServiceAppointment.API.Interfaces
{
    public interface IRabbitMqPublisher
    {
        void PublishNotification(object message);
    }
}
