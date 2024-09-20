using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Orders.Data.Entities
{
    public class OrderItem : BaseEntity
    {
        public required string ProductId { get; set; }
        public required int Quantity { get; set; }
        public required string OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
