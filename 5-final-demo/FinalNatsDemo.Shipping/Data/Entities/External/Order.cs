using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Shipping.Data.Entities.External
{
    public class Order : BaseEntity
    {
        public new required string Id { get; set; }
        public required string OrderStatus { get; set; }
        public required List<OrderItem> Items { get; set; }
    }
}
