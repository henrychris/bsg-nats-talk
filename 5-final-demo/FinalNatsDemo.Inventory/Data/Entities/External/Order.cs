using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Inventory.Data.Entities.External
{
    public class Order : BaseEntity
    {
        public new required string Id { get; set; }
        public required string OrderStatus { get; set; }
        public List<OrderItem> Items { get; set; } = [];
    }
}
