using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NatsIdempotentDemo.Common;

var opts = new NatsOpts { Url = NatsConfig.DefaultUrl };
await using var connection = new NatsConnection(opts);
var messageCount = 0;

// setup stream here or use cli


var jsContext = new NatsJSContext(connection);
var config = new StreamConfig(
    name: NatsConfig.StreamName,
    subjects: new[] { NatsConfig.SubjectName }
)
{
    Storage = StreamConfigStorage.Memory, // messages will be lost once the nats server is reset.
};
await jsContext.CreateStreamAsync(config);

while (true)
{
    var message = new Message
    {
        Id = Guid.NewGuid().ToString(),
        Content = $"Message #{messageCount} at {DateTime.Now}",
        Timestamp = DateTime.Now,
    };

    Console.WriteLine("Hit any key to publish a message.");
    var key = Console.ReadKey(true);

    if (key.Key == ConsoleKey.Escape) // If Escape is pressed, you can break the loop if needed
    {
        return;
    }

    await jsContext.PublishAsync(
        NatsConfig.SubjectName,
        message,
        serializer: new NatsSerializer<Message>()
    );

    Console.WriteLine($"Published message #{messageCount}.");
    messageCount++;
}
