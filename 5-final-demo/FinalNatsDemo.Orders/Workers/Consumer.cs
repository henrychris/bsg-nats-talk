using FinalNatsDemo.Common;
using FinalNatsDemo.Common.Nats;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace FinalNatsDemo.Orders.Workers
{
    public class Consumer(ILogger<Consumer> logger, INatsWrapper natsWrapper, NatsConnection natsConnection) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var streamName = "test-message-stream-1";
            var subject = "test.message";

            var jsContext = new NatsJSContext(natsConnection);
            await CreateStreamAsync(jsContext, streamName, subject);

            logger.LogInformation("Consumer started, waiting for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await natsWrapper.ConsumeFromJetStreamAsync<Message>(
                    async (message) =>
                    {
                        logger.LogInformation($"Message received: {message.Id}");
                        await Task.CompletedTask;
                    },
                    streamName: streamName,
                    cancellationToken: stoppingToken
                );
            }
        }

        private async Task CreateStreamAsync(NatsJSContext jsContext, string streamName, string subject)
        {
            const int DUPLICATE_WINDOW_IN_SECONDS = 3;

            // create stream
            var config = new StreamConfig(name: streamName, subjects: new[] { subject })
            {
                Storage = StreamConfigStorage.Memory, // messages will be lost once the nats server is reset.
                DuplicateWindow = TimeSpan.FromSeconds(DUPLICATE_WINDOW_IN_SECONDS),
            };
            await TryCreateOrUpdateStreamAsync(jsContext, config);
        }

        static async Task TryCreateOrUpdateStreamAsync(NatsJSContext jsContext, StreamConfig config)
        {
            try
            {
                await jsContext.CreateStreamAsync(config);
            }
            catch (Exception)
            {
                await jsContext.UpdateStreamAsync(config);
            }
        }
    }
}
