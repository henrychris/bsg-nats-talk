using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Shipping.Data;
using FinalNatsDemo.Shipping.Data.Entities;
using FinalNatsDemo.Shipping.Data.Entities.External;
using FinalNatsDemo.Shipping.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Shipping.EventHandlers
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
                    cancellationToken: stoppingToken
                );
            }
        }

        private async Task HandleOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ShippingDataContext>();

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

            var shippingRecord = new ShippingRecord
            {
                OrderId = orderCreatedEvent.OrderId,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                Status = ShippingStatus.Scheduled,
            };

            var order = new Order { Id = orderCreatedEvent.OrderId, OrderStatus = orderCreatedEvent.OrderStatus };
            var orderItems = orderCreatedEvent.Items.Select(x => new OrderItem
            {
                Id = x.OrderItemId,
                OrderId = order.Id,
                Quantity = x.Quantity,
                ProductId = x.ProductId,
            });

            await context.ShippingRecords.AddAsync(shippingRecord, cancellationToken);
            await context.Orders.AddAsync(order, cancellationToken);
            await context.OrderItems.AddRangeAsync(orderItems, cancellationToken);

            // Save changes to the product stock levels
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
