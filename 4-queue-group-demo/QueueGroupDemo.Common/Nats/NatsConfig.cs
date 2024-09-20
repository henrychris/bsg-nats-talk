namespace QueueGroupDemo.Common.Nats;

public static class NatsConfig
{
    public const string DefaultUrl = "nats://localhost:4222";
    public const string SubjectName = "demo.queuegroup.3";
    public const string StreamName = "demo-queuegroup-stream-3";
}
