namespace FinalNatsDemo.Inventory.Features.Products.GetAllProducts
{
    public class GetProductResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required int StockLevel { get; set; }
        public required DateTime DateCreated { get; set; }
        public required DateTime DateUpdated { get; set; }
    }
}
