using FinalNatsDemo.Common.Data.Entities.Base;
using FinalNatsDemo.Orders.Data.Enums;

namespace FinalNatsDemo.Orders.Data.Entities
{
    public class Order : BaseEntity
    {
        public required OrderStatus Status { get; set; }
        public required List<OrderItem> Items { get; set; } = [];
    }
}
