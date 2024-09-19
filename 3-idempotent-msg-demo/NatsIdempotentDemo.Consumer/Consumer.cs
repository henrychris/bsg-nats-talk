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
            new ConsumerConfig()
            {
                DeliverPolicy =
                    ConsumerConfigDeliverPolicy.New // change this to change how messages are retrieved
                ,
                Backoff =
                [
                    (long)TimeSpan.FromSeconds(5).TotalNanoseconds, // 5 seconds
                    (long)TimeSpan.FromSeconds(10).TotalNanoseconds, // 10 seconds
                    (long)TimeSpan.FromSeconds(30).TotalNanoseconds, // 30 seconds
                    (long)TimeSpan.FromMinutes(2).TotalNanoseconds, // 2 minutes
                    (long)TimeSpan.FromMinutes(5).TotalNanoseconds, // 5 minutes
                    (long)
                        TimeSpan
                            .FromMinutes(10)
                            .TotalNanoseconds // 10 minutes
                    ,
                ],
                AckWait = TimeSpan.FromSeconds(10),
                MaxDeliver = 7,
            },
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
                    await msg.AckTerminateAsync(cancellationToken: stoppingToken);
                    continue;
                }

                logger.LogInformation(
                    "Received: {messageId} - {message.Content}",
                    msg.Data.Id,
                    msg.Data.Content
                );

                await msg.NakAsync(delay: TimeSpan.FromSeconds(5), cancellationToken: stoppingToken);
            }
        }
    }
}
