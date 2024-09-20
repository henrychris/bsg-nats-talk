using NATS.Client.Core;
using QueueGroupDemo.Common;
using QueueGroupDemo.Common.Nats;

namespace QueueGroupDemo.Consumers
{
    public class Subscriber(ILogger<Subscriber> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opts = new NatsOpts { Url = NatsConfig.DefaultUrl, Name = "Subscriber 1" };
            await using var nats = new NatsConnection(opts);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var subscription = nats.SubscribeAsync<Message>(
                        NatsConfig.SubjectName,
                        queueGroup: "subscriber-queue",
                        serializer: new NatsSerializer<Message>(),
                        cancellationToken: stoppingToken
                    );

                    await foreach (var msg in subscription)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;

                        if (msg.Data is null)
                        {
                            continue;
                        }

                        Console.WriteLine(
                            $"{nameof(Subscriber)} received message on subject: {msg.Subject}.\nReceived: {msg.Data.Id} - {msg.Data.Content}"
                        );

                        Console.ResetColor();
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(
                        "Something went wrong in the handler for {subject}.\nDetails: {exception}",
                        NatsConfig.SubjectName,
                        e
                    );
                }
            }
        }
    }
}
