using System.Reflection;
using System.Text.Json;
using Confluent.Kafka;
using Shared.Messages;

namespace ChangeTracker.BaseServices;

public class MessagePubliserService
{
    private readonly ILogger<MessagePubliserService> _logger;

    private readonly IProducer<string, string> _producer;

    public MessagePubliserService(
        ILogger<MessagePubliserService> logger,
        IProducer<string, string> producer
    )
    {
        _logger = logger;
        _producer = producer;
    }

    public async Task PublishMessageAsync<T>(
        T message,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Simulate publishing the message to a message broker or service bus
            await _producer.ProduceAsync(
                typeof(T).GetCustomAttribute<ChannelAttribute>()?.TopicName,
                new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = JsonSerializer.Serialize(message),
                },
                cancellationToken
            );

            _logger.LogInformation(
                $"Published message to topic '{typeof(T).GetCustomAttribute<ChannelAttribute>()?.TopicName}': {message}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"Failed to publish message to topic '{typeof(T).GetCustomAttribute<ChannelAttribute>()?.TopicName}'"
            );
            throw;
        }
    }
}
