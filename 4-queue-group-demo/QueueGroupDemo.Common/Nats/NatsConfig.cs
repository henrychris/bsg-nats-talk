namespace QueueGroupDemo.Common.Nats;

public static class NatsConfig
{
    public const string DefaultUrl = "nats://localhost:4222";
    public const string SubjectName = "demo.messages.3";
    public const string StreamName = "demostream-3";
}
