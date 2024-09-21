using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Shipping.Data.Entities.External;

namespace FinalNatsDemo.Shipping.EventHandlers
{
    public static class EventMapper
    {
        internal static Product CreateProduct(ProductCreatedEvent productCreatedEvent)
        {
            return new Product
            {
                Id = productCreatedEvent.Id,
                Name = productCreatedEvent.Name,
                StockLevel = productCreatedEvent.StockLevel,
            };
        }
    }
}
