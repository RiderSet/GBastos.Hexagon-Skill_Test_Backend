using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GBastos.Hexagon_Skill_Test.Api.Messaging;

public class RabbitMQPublisher
{
    private readonly string _hostname;
    private readonly string _queueName;

    public RabbitMQPublisher(IConfiguration configuration)
    {
        _hostname = configuration["RabbitMQ:HostName"]
            ?? throw new ArgumentNullException("RabbitMQ:HostName não configurado");
        _queueName = configuration["RabbitMQ:QueueName"]
            ?? throw new ArgumentNullException("RabbitMQ:QueueName não configurado");
    }

    public void Publish<T>(T message)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };

        using IConnection connection = factory.CreateConnection();
        using IModel channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var messageString = JsonSerializer.Serialize(message) ?? string.Empty;
        var body = Encoding.UTF8.GetBytes(messageString);

        channel.BasicPublish(
            exchange: "",
            routingKey: _queueName,
            basicProperties: null,
            body: body);
    }
}