using FinalNatsDemo.Common.Data.Entities.Base;
using FinalNatsDemo.Orders.Data.Entities.External;

namespace FinalNatsDemo.Orders.Data.Entities
{
    public class OrderItem : BaseEntity
    {
        public required string ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public required int Quantity { get; set; }
        public required string OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
