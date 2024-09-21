using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Orders.Data.Entities.External;

namespace FinalNatsDemo.Orders.EventHandlers
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
