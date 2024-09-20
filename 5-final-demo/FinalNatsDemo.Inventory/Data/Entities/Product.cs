using FinalNatsDemo.Common.Data.Entities.Base;

namespace FinalNatsDemo.Inventory.Data.Entities
{
    public class Product : BaseEntity
    {
        public required string Name { get; set; }
        public required int StockLevel { get; set; }
    }
}
