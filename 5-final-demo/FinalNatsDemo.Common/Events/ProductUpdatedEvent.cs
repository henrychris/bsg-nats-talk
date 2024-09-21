namespace FinalNatsDemo.Common.Events
{
    public class ProductUpdatedEvent
    {
        public const string TOPIC = "product.updated.event";
        public const string STREAM = "product-updated-stream";
        public required string ProductId { get; set; }
        public required int StockLevel { get; set; }
    }
}
