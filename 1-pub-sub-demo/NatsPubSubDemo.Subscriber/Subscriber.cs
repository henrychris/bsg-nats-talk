using NATS.Client.Core;
using NatsPubSubDemo.Common;

namespace NatsPubSubDemo.Subscriber;

public class Subscriber(ILogger<Subscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = new NatsOpts { Url = NatsConfig.DefaultUrl };
        await using var connection = new NatsConnection(opts);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Subscribing to topic: {topic}.", NatsConfig.SubjectName);
            await foreach (
                var msg in connection.SubscribeAsync<Message>(
                    NatsConfig.SubjectName,
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
