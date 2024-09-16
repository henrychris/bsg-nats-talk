namespace NatsServiceOutageDemo.Common;

public class Message
{
    public required string Id { get; set; }
    public required string Content { get; set; }
    public required DateTime Timestamp { get; set; }
}
