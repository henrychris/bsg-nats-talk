using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Shipping.Data.Entities.External
{
    public class OrderItem : BaseEntity
    {
        public new required string Id { get; set; }
        public required string ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public required int Quantity { get; set; }
        public required string OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
