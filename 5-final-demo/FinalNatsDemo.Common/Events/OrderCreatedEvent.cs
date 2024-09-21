namespace FinalNatsDemo.Common.Events
{
    public class OrderCreatedEvent
    {
        public const string TOPIC = "order.created.event";
        public const string STREAM = "order-created-stream";
        public required string OrderId { get; set; }
        public required string OrderStatus { get; set; }
        public required List<OrderItemEvent> Items { get; set; }
    }

    public class OrderItemEvent
    {
        public required string OrderItemId { get; set; }
        public required string ProductId { get; set; }
        public required int Quantity { get; set; }
    }
}
