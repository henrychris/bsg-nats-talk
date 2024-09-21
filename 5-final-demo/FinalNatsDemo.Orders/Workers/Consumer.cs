using FinalNatsDemo.Common;
using FinalNatsDemo.Common.Nats;

namespace FinalNatsDemo.Orders.Workers
{
    public class Consumer(ILogger<Consumer> logger, INatsWrapper natsWrapper) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var streamName = "test-message-stream-1";
            var subject = "test.message";

            await natsWrapper.CreateStreamAsync(streamName, subject);

            logger.LogInformation("Consumer started, waiting for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await natsWrapper.ConsumeFromJetStreamAsync<Message>(
                    async (message) =>
                    {
                        logger.LogInformation("Message received: {messageId}", message.Id);
                        await Task.CompletedTask;
                    },
                    streamName: streamName,
                    cancellationToken: stoppingToken
                );
            }
        }
    }
}
