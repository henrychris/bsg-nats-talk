using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace FinalNatsDemo.Common.Nats
{
	public interface INatsWrapper
	{
		Task ConsumeFromJetStreamAsync<TMessage>(Func<TMessage, Task> onMessage, string streamName, int maxMessagesInBuffer = 1, CancellationToken cancellationToken = default);
	}

	public class NatsWrapper : INatsWrapper
	{
		private readonly ILogger<NatsWrapper> _logger;
		private readonly INatsConnection _connection;
		private readonly NatsJSContext _natsJsContext;

		public NatsWrapper(
			INatsConnectionPool natsConnectionPool,
			ILogger<NatsWrapper> logger,
			NatsJSContext natsJsContext
		)
		{
			_logger = logger;
			_natsJsContext = natsJsContext;
			_connection = natsConnectionPool.GetConnection();
			_connection.ConnectAsync();
		}

		public async Task ConsumeFromJetStreamAsync<TMessage>(
			Func<TMessage, Task> onMessage,
			string streamName,
			int maxMessagesInBuffer = 1,
			CancellationToken cancellationToken = default
		)
		{
			_logger.LogInformation("Listening to stream: {streamName}.", streamName);

			var consumer = await CreateConsumerAsync(streamName, cancellationToken);
			await ConsumeMessagesAsync(onMessage, consumer, maxMessagesInBuffer, cancellationToken);
		}

		private async Task<INatsJSConsumer> CreateConsumerAsync(
			string streamName,
			CancellationToken cancellationToken
		)
		{
			// var consumerName = $"{streamName}_processor";
			var consumerConfig = new ConsumerConfig()
			{
				ReplayPolicy = ConsumerConfigReplayPolicy.Instant,
			};

			_logger.LogInformation("Creating consumer for stream: {streamName}.", streamName);
			return await _natsJsContext.CreateOrUpdateConsumerAsync(
				streamName,
				consumerConfig,
				cancellationToken
			);
		}

		private async Task ConsumeMessagesAsync<TMessage>(
			Func<TMessage, Task> onMessage,
			INatsJSConsumer consumer,
			int maxMessagesInBuffer,
			CancellationToken cancellationToken
		)
		{
			var consumeOpts = new NatsJSConsumeOpts { MaxMsgs = maxMessagesInBuffer };

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
	}
}
