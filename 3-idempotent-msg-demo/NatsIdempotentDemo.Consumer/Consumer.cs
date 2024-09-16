using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NatsIdempotentDemo.Common;

namespace NatsIdempotentDemo.Consumer;

public class Consumer(ILogger<Consumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = new NatsOpts { Url = NatsConfig.DefaultUrl };
        await using var connection = new NatsConnection(opts);

        var jsContext = new NatsJSContext(connection);
        var stream = await jsContext.GetStreamAsync(
            NatsConfig.StreamName,
            cancellationToken: stoppingToken
        );
        var consumer = await stream.CreateOrUpdateConsumerAsync(
            new ConsumerConfig(),
            stoppingToken
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Subscribing to topic: {topic}.", NatsConfig.SubjectName);
            await foreach (
                var msg in consumer.ConsumeAsync<Message>(
                    serializer: new NatsSerializer<Message>(),
                    cancellationToken: stoppingToken
                )
            )
            {
                Console.WriteLine($"Received {msg.Subject}: {msg.Data}\n");

                if (msg.Data is null)
                {
                    logger.LogError("Message is null.");
                    continue;
                }

                logger.LogInformation(
                    "Received: {messageId} - {message.Content}",
                    msg.Data.Id,
                    msg.Data.Content
                );
            }
        }
    }
}
