using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Shared.ConfigurateObjects;
using Shared.Messages;

namespace ConsumerDisplayWebApp.Jobs;

public class BaseConsumerJob<TMessage> : BackgroundService
{
    private readonly ILogger<BaseConsumerJob<TMessage>> _logger;
    private readonly IConsumer<string, string> _consumer;

    public BaseConsumerJob(
        ILogger<BaseConsumerJob<TMessage>> logger,
        IConsumer<string, string> consumer
    )
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken canellationToken)
    {
        try
        {
            var topicAttribute =
                typeof(TMessage)
                    .GetCustomAttributes(typeof(ChannelAttribute), false)
                    .FirstOrDefault() as ChannelAttribute;

            if (topicAttribute == null)
            {
                throw new InvalidOperationException(
                    $"ChannelAttribute is not defined for {typeof(TMessage).Name}"
                );
            }

            _consumer.Subscribe(topicAttribute.TopicName);
            _logger.LogInformation(
                "Kafka consumer started. Topic: {Topic}",
                topicAttribute.TopicName
            );

            while (!canellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(canellationToken);

                    if (consumeResult != null)
                    {
                        _logger.LogInformation(
                            "Consumed message from topic '{Topic}': {Message}",
                            consumeResult.Topic,
                            consumeResult.Message.Value
                        );
                    }

                    _consumer.StoreOffset(consumeResult);
                    _consumer.Commit();
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation gracefully
                    _logger.LogInformation("Consumer job is stopping due to cancellation.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while consuming messages.");
                }

                await Task.Delay(10000, canellationToken); // Adjust the delay as needed
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped. Topic: {Topic}", nameof(TMessage));
        }
    }
}
