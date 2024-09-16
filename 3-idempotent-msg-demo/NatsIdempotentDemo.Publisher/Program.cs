using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NatsIdempotentDemo.Common;

var opts = new NatsOpts { Url = NatsConfig.DefaultUrl };
await using var connection = new NatsConnection(opts);
var messageCount = 0;

var jsContext = new NatsJSContext(connection);
var config = new StreamConfig(
    name: NatsConfig.StreamName,
    subjects: new[] { NatsConfig.SubjectName }
)
{
    Storage = StreamConfigStorage.Memory, // messages will be lost once the nats server is reset.
    DuplicateWindow = TimeSpan.FromSeconds(3),
};

await CreateStream(jsContext, config);

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

    var messageId = Guid.NewGuid().ToString();
    var jsPubOpts = new NatsJSPubOpts { MsgId = messageId };

    await jsContext.PublishAsync(
        NatsConfig.SubjectName,
        message,
        serializer: new NatsSerializer<Message>(),
        opts: jsPubOpts
    );
    Console.WriteLine("published message");

    await Task.Delay(4000);

    await jsContext.PublishAsync(
        NatsConfig.SubjectName,
        message,
        serializer: new NatsSerializer<Message>(),
        opts: jsPubOpts
    );
    Console.WriteLine("published duplicate message");

    Console.WriteLine($"Published message #{messageCount}.");
    messageCount++;
}

static async Task CreateStream(NatsJSContext jsContext, StreamConfig config)
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
