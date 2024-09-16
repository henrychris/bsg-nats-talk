using NATS.Client.Core;
using NatsServiceOutageDemo.Common;

var opts = new NatsOpts { Url = NatsConfig.DefaultUrl };
await using var connection = new NatsConnection(opts);
var messageCount = 0;

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

    await connection.PublishAsync(
        NatsConfig.SubjectName,
        message,
        serializer: new NatsSerializer<Message>()
    );

    Console.WriteLine($"Published message #{messageCount}.");
    messageCount++;
}
