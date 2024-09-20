namespace FinalNatsDemo.Common
{
    public static class NatsConfig
    {
        public const string DefaultUrl = "nats://localhost:4222";

        public const string OrderCreatedSubject = "order.created";
        public const string OrderCreatedStream = "order.created.stream";

        public const string OrderProcessedSubject = "order.processed";
        public const string OrderProcessedStream = "order.processed.stream";
        
        public const string OrderShippedSubject = "order.shipped";
        public const string OrderShippedStream = "order.shipped.stream";
    }
}
