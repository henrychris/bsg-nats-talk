using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Shipping.Data.Entities.External
{
    public class Product : BaseEntity
    {
        public new required string Id { get; set; }
        public required string Name { get; set; }
        public required int StockLevel { get; set; }
    }
}
