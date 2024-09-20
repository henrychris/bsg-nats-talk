using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Inventory.Data.Entities.External
{
    public class OrderItem : BaseEntity
    {
        public new required string Id { get; set; }
        public required string ProductId { get; set; }
        public required int Quantity { get; set; }
        public required string OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
