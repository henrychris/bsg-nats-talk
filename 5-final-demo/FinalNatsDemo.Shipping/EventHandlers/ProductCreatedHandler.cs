using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Shipping.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Shipping.EventHandlers
{
    public class ProductCreatedHandler(INatsWrapper natsWrapper, IServiceScopeFactory serviceScopeFactory, ILogger<ProductCreatedHandler> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await natsWrapper.CreateStreamAsync(ProductCreatedEvent.STREAM, ProductCreatedEvent.TOPIC);

            logger.LogInformation("Consumer started, waiting for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await natsWrapper.ConsumeFromJetStreamAsync<ProductCreatedEvent>(
                    async (message) => await CreateProductAsync(message, stoppingToken),
                    streamName: ProductCreatedEvent.STREAM,
                    cancellationToken: stoppingToken
                );
            }
        }

        private async Task CreateProductAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken)
        {
            using var messageScope = serviceScopeFactory.CreateScope();
            var context = messageScope.ServiceProvider.GetRequiredService<ShippingDataContext>();

            var doesProductExist = await context.Products.AnyAsync(x => x.Id == productCreatedEvent.Id, cancellationToken: cancellationToken);
            if (doesProductExist)
            {
                logger.LogWarning("Product already exists. Id: {productId}. Skipping...", productCreatedEvent.Id);
                return;
            }

            var product = EventMapper.CreateProduct(productCreatedEvent);
            await context.Products.AddAsync(product, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Created product. Id: {productId}", product.Id);
        }
    }
}
