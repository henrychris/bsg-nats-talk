using FinalNatsDemo.Common.Data.Entities.Base;
using FinalNatsDemo.Shipping.Data.Entities.External;
using FinalNatsDemo.Shipping.Data.Enums;

namespace FinalNatsDemo.Shipping.Data.Entities
{
    public class ShippingRecord : BaseEntity
    {
        public required DateTime ScheduledDate { get; set; }
        public required ShippingStatus Status { get; set; }
        public required string OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
