namespace NatsServiceOutageDemo.Common;

public static class NatsConfig
{
    public const string DefaultUrl = "nats://localhost:4222";
    public const string SubjectName = "demo.messages";
    public const string StreamName = "demostream";
}
