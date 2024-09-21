using FinalNatsDemo.Common.Nats.Serializer;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace FinalNatsDemo.Common.Nats
{
    public interface INatsWrapper
    {
        Task CreateStreamAsync(string streamName, string subject);

        Task PublishAsync<TMessage>(
            TMessage message,
            string subject,
            CancellationToken cancellationToken = default
        );
        Task PublishToJetStreamAsync<TMessage>(
            TMessage message,
            string subject,
            string messageId,
            CancellationToken cancellationToken = default
        );

        Task ConsumeFromJetStreamAsync<TMessage>(
            Func<TMessage, Task> onMessage,
            string streamName,
            CancellationToken cancellationToken = default
        );
    }

    public class NatsWrapper : INatsWrapper
    {
        private const int DUPLICATE_WINDOW_IN_SECONDS = 3;
        private readonly ILogger<NatsWrapper> _logger;
        private readonly NatsConnection _connection;
        private readonly NatsJSContext _natsJsContext;

        public NatsWrapper(NatsConnection connection, ILogger<NatsWrapper> logger)
        {
            _logger = logger;
            _connection = connection;
            _natsJsContext = new NatsJSContext(_connection);

            _connection.ConnectAsync();
        }

        #region PUBLISH

        public async Task PublishAsync<TMessage>(
            TMessage message,
            string subject,
            CancellationToken cancellationToken = default
        )
        {
            await _connection.PublishAsync(
                subject,
                message,
                serializer: new NatsSerializer<TMessage>(),
                cancellationToken: cancellationToken
            );
            _logger.LogInformation("Published messaged to NATS.");
        }

        public async Task PublishToJetStreamAsync<TMessage>(
            TMessage message,
            string subject,
            string messageId,
            CancellationToken cancellationToken = default
        )
        {
            var jsPubOpts = new NatsJSPubOpts
            {
                RetryAttempts = 5,
                RetryWaitBetweenAttempts = TimeSpan.FromSeconds(5),
                MsgId = messageId,
            };

            var ack = await _natsJsContext.PublishAsync(
                subject,
                message,
                new NatsSerializer<TMessage>(),
                jsPubOpts,
                cancellationToken: cancellationToken
            );
            if (ack.Duplicate)
            {
                _logger.LogError(
                    "Message not sent as it is a duplicate. Subject: {subject}.",
                    subject
                );
                return;
            }

            ack.EnsureSuccess();
            _logger.LogInformation(
                "Published message to JetStream for subject: {subject}.",
                subject
            );
        }

        #endregion

        #region CONSUME

        public async Task ConsumeFromJetStreamAsync<TMessage>(
            Func<TMessage, Task> onMessage,
            string streamName,
            CancellationToken cancellationToken = default
        )
        {
            var consumer = await CreateConsumerAsync(streamName, cancellationToken);
            _logger.LogInformation("Created consumer for stream: {streamName}.", streamName);

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Fetching messages from stream: {stream}.", streamName);
                await foreach (
                    var msg in consumer.ConsumeAsync<TMessage>(
                        serializer: new NatsSerializer<TMessage>(),
                        cancellationToken: cancellationToken
                    )
                )
                {
                    _logger.LogInformation("Received message on topic {topic}.", msg.Subject);

                    if (msg.Data is null)
                    {
                        _logger.LogError("Message is null.");
                        await msg.AckTerminateAsync(cancellationToken: cancellationToken);
                        continue;
                    }

                    await onMessage(msg.Data);
                    await msg.AckAsync(cancellationToken: cancellationToken);
                }
            }
        }

        private async Task<INatsJSConsumer> CreateConsumerAsync(
            string streamName,
            CancellationToken cancellationToken
        )
        {
            var consumerConfig = new ConsumerConfig()
            {
                ReplayPolicy = ConsumerConfigReplayPolicy.Original,
                DeliverPolicy = ConsumerConfigDeliverPolicy.New,
            };

            return await _natsJsContext.CreateOrUpdateConsumerAsync(
                streamName,
                consumerConfig,
                cancellationToken
            );
        }

        #endregion

        public async Task CreateStreamAsync(string streamName, string subject)
        {
            // create stream
            var config = new StreamConfig(name: streamName, subjects: new[] { subject })
            {
                Storage = StreamConfigStorage.Memory, // messages will be lost once the nats server is reset.
                DuplicateWindow = TimeSpan.FromSeconds(DUPLICATE_WINDOW_IN_SECONDS),
            };
            await TryCreateOrUpdateStreamAsync(config);
        }

        private async Task TryCreateOrUpdateStreamAsync(StreamConfig config)
        {
            try
            {
                await _natsJsContext.CreateStreamAsync(config);
            }
            catch (Exception)
            {
                await _natsJsContext.UpdateStreamAsync(config);
            }
        }
    }
}
