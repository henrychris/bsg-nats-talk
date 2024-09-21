using FinalNatsDemo.Common.Nats.Serializer;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace FinalNatsDemo.Common.Nats
{
    public interface INatsWrapper
    {
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
            _logger.LogInformation("Listening to stream: {streamName}.", streamName);

            var consumer = await CreateConsumerAsync(streamName, cancellationToken);
            _logger.LogInformation("Created consumer for stream: {streamName}.", streamName);

            await ConsumeMessagesAsync(onMessage, consumer, cancellationToken);
        }

        private async Task<INatsJSConsumer> CreateConsumerAsync(
            string streamName,
            CancellationToken cancellationToken
        )
        {
            var consumerConfig = new ConsumerConfig()
            {
                ReplayPolicy = ConsumerConfigReplayPolicy.Instant,
            };

            return await _natsJsContext.CreateOrUpdateConsumerAsync(
                streamName,
                consumerConfig,
                cancellationToken
            );
        }

        private async Task ConsumeMessagesAsync<TMessage>(
            Func<TMessage, Task> onMessage,
            INatsJSConsumer consumer,
            CancellationToken cancellationToken
        )
        {
            var consumeOpts = new NatsJSConsumeOpts { MaxMsgs = 1 };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (
                        var msg in consumer.ConsumeAsync(
                            serializer: new NatsSerializer<TMessage>(),
                            consumeOpts,
                            cancellationToken: cancellationToken
                        )
                    )
                    {
                        await HandleMessage(msg, onMessage, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning(
                        "Consumption was cancelled for consumer: {consumer}.",
                        consumer
                    );
                }
                catch (Exception e)
                {
                    _logger.LogCritical(
                        "Error processing messages: {exception}. Retrying in 1 second.",
                        e
                    );
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private async Task HandleMessage<TMessage>(
            NatsJSMsg<TMessage> msg,
            Func<TMessage, Task> onMessage,
            CancellationToken cancellationToken
        )
        {
            try
            {
                if (msg.Data is null)
                {
                    await msg.NakAsync(cancellationToken: cancellationToken);
                    return;
                }

                await onMessage(msg.Data);
                await msg.AckAsync(cancellationToken: cancellationToken);
                _logger.LogInformation("Message successfully processed and acknowledged.");
            }
            catch (Exception e)
            {
                _logger.LogError("Error processing message: {exception}. NACKing the message.", e);
                await msg.NakAsync(cancellationToken: cancellationToken);
            }
        }

        #endregion
    }
}
