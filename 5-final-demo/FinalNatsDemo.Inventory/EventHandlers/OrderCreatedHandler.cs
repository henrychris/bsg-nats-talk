using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Inventory.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Inventory.EventHandlers
{
    public class OrderCreatedHandler(INatsWrapper natsWrapper, IServiceScopeFactory serviceScopeFactory, ILogger<OrderCreatedHandler> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await natsWrapper.CreateStreamAsync(OrderCreatedEvent.STREAM, OrderCreatedEvent.TOPIC);

            logger.LogInformation("Consumer started, waiting for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await natsWrapper.ConsumeFromJetStreamAsync<OrderCreatedEvent>(
                    async (message) => await HandleOrderCreatedAsync(message, stoppingToken),
                    streamName: OrderCreatedEvent.STREAM,
                    "inventory-order-created-consumer",
                    cancellationToken: stoppingToken
                );
            }
        }

        private async Task HandleOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InventoryDataContext>();

            logger.LogInformation("Received OrderCreatedEvent for OrderId: {OrderId}", orderCreatedEvent.OrderId);

            var productIds = orderCreatedEvent.Items.Select(item => item.ProductId).ToList();
            var products = await context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);

            foreach (var orderItem in orderCreatedEvent.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
                if (product == null)
                {
                    logger.LogError("Product not found for ProductId: {ProductId}", orderItem.ProductId);
                    continue; // Skip to the next item if the product is not found
                }

                product.StockLevel -= orderItem.Quantity;

                logger.LogInformation("Decrementing stock for ProductId: {ProductId}. New StockLevel: {StockLevel}", product.Id, product.StockLevel);

                if (product.StockLevel < 0)
                {
                    logger.LogWarning("Stock level for ProductId: {ProductId} is negative. StockLevel: {StockLevel}", product.Id, product.StockLevel);
                }
            }

            // Save changes to the product stock levels
            await context.SaveChangesAsync(cancellationToken);

            // Publish ProductUpdatedEvent for each product with updated stock level
            foreach (var product in products)
            {
                var productUpdatedEvent = new ProductUpdatedEvent { ProductId = product.Id, StockLevel = product.StockLevel };
                var messageId = Guid.NewGuid().ToString();

                await natsWrapper.PublishToJetStreamAsync(productUpdatedEvent, ProductUpdatedEvent.TOPIC, messageId, cancellationToken);
                logger.LogInformation("Published ProductUpdatedEvent for ProductId: {ProductId}", product.Id);
            }
        }
    }
}
