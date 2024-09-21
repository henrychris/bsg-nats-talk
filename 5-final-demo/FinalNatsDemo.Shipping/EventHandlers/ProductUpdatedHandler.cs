using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Shipping.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Shipping.EventHandlers
{
    public class ProductUpdatedHandler(INatsWrapper natsWrapper, IServiceScopeFactory serviceScopeFactory, ILogger<ProductUpdatedHandler> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await natsWrapper.CreateStreamAsync(ProductUpdatedEvent.STREAM, ProductUpdatedEvent.TOPIC);

            logger.LogInformation("Consumer started, waiting for ProductUpdatedEvent messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await natsWrapper.ConsumeFromJetStreamAsync<ProductUpdatedEvent>(
                    async (message) => await HandleProductUpdatedAsync(message, stoppingToken),
                    streamName: ProductUpdatedEvent.STREAM,
                    cancellationToken: stoppingToken
                );
            }
        }

        private async Task HandleProductUpdatedAsync(ProductUpdatedEvent productUpdatedEvent, CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ShippingDataContext>();

            logger.LogInformation(
                "Received ProductUpdatedEvent for ProductId: {ProductId}, StockLevel: {StockLevel}",
                productUpdatedEvent.ProductId,
                productUpdatedEvent.StockLevel
            );

            // Retrieve the product in the order database
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productUpdatedEvent.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogError("Product not found for ProductId: {ProductId}", productUpdatedEvent.ProductId);
                return;
            }

            product.StockLevel = productUpdatedEvent.StockLevel;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Product stock level updated for ProductId: {ProductId}. New StockLevel: {StockLevel}",
                product.Id,
                product.StockLevel
            );
        }
    }
}
