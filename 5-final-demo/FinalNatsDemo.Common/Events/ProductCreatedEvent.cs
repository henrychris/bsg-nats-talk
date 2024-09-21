namespace FinalNatsDemo.Common.Events
{
    public class ProductCreatedEvent
    {
        public const string TOPIC = "product.created.event";
        public const string STREAM = "product-created-stream";
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required int StockLevel { get; set; }
    }
}
