using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GBastos.Hexagon_Skill_Test.Api.Messaging.Brokers;

public class RabbitMQPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMQPublisher(IConfiguration configuration)
    {
        var hostname = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("HostName");
        _queueName = configuration["RabbitMQ:QueueName"] ?? throw new ArgumentNullException("QueueName");

        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            UserName = configuration["RabbitMQ:Username"],
            Password = configuration["RabbitMQ:Password"]
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar ao RabbitMQ: {ex.Message}");
            throw;
        }
    }

    public void Publish<T>(T message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: "",
            routingKey: _queueName,
            basicProperties: properties,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        GC.SuppressFinalize(this);
    }
}