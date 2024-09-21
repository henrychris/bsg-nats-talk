using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Orders.Data;
using FinalNatsDemo.Orders.Data.Entities;
using FinalNatsDemo.Orders.Data.Entities.External;
using FinalNatsDemo.Orders.Data.Enums;
using FluentValidation;
using HenryUtils.Extensions;
using HenryUtils.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Orders.Features.Orders.CreateOrder
{
    public class CreateOrderRequest : IRequest<Result<CreateOrderResponse>>
    {
        public List<OrderItemDto> OrderItems { get; set; } = [];
    }

    public class OrderItemDto
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class Handler(OrderDataContext context, INatsWrapper natsWrapper, IValidator<CreateOrderRequest> validator, ILogger<Handler> logger)
        : IRequestHandler<CreateOrderRequest, Result<CreateOrderResponse>>
    {
        public async Task<Result<CreateOrderResponse>> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var err = validationResult.ToErrorList();
                logger.LogError("Validation failed for request.\nErrors: {errors}.", err);
                return Result<CreateOrderResponse>.Failure(err);
            }

            var products = await GetProductsForOrder(request.OrderItems, cancellationToken);
            var stockCheckResult = CheckStockAvailability(request.OrderItems, products);
            if (!stockCheckResult.IsSuccess)
            {
                return Result<CreateOrderResponse>.Failure(stockCheckResult.Error);
            }

            var order = CreateOrder(request.OrderItems, products);
            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);

            await PublishOrderCreatedEvent(order, cancellationToken);

            logger.LogInformation("Order created successfully. OrderId: {OrderId}", order.Id);
            return Result<CreateOrderResponse>.Success(new CreateOrderResponse { OrderId = order.Id });
        }

        private async Task<List<Product>> GetProductsForOrder(IEnumerable<OrderItemDto> orderItems, CancellationToken cancellationToken)
        {
            var productIds = orderItems.Select(x => x.ProductId).ToList();
            return await context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);
        }

        private Result<MyUnit> CheckStockAvailability(IEnumerable<OrderItemDto> orderItems, List<Product> products)
        {
            List<string> insufficientStockItems = [];
            foreach (var orderItem in orderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
                if (product == null)
                {
                    logger.LogError("Product not found: {ProductId}", orderItem.ProductId);
                    return Result<MyUnit>.Failure(Error.NotFound("Product.NotFound", "Product not found."));
                }

                if (product.StockLevel < orderItem.Quantity)
                {
                    insufficientStockItems.Add(product.Name);
                }
            }

            if (insufficientStockItems.Count != 0)
            {
                var message = $"Insufficient stock for products: {string.Join(", ", insufficientStockItems)}";
                logger.LogError("{message}", message);
                return Result<MyUnit>.Failure(Error.Failure("InsufficientStock", message));
            }

            return Result<MyUnit>.Success(MyUnit.Value);
        }

        private static Order CreateOrder(IEnumerable<OrderItemDto> orderItems, List<Product> products)
        {
            var order = new Order { Status = OrderStatus.Created, Items = [] };

            foreach (var orderItem in orderItems)
            {
                var product = products.First(p => p.Id == orderItem.ProductId);
                product.StockLevel -= orderItem.Quantity;
                var orderItemEntity = new OrderItem
                {
                    ProductId = orderItem.ProductId,
                    Quantity = orderItem.Quantity,
                    OrderId = order.Id,
                };
                order.Items.Add(orderItemEntity);
            }

            return order;
        }

        private async Task PublishOrderCreatedEvent(Order order, CancellationToken cancellationToken)
        {
            var messageId = Guid.NewGuid().ToString();

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                OrderStatus = order.Status.ToString(),
                Items = order
                    .Items.Select(x => new OrderItemEvent
                    {
                        OrderItemId = x.Id,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                    })
                    .ToList(),
            };
            await natsWrapper.PublishToJetStreamAsync(orderCreatedEvent, OrderCreatedEvent.TOPIC, messageId, cancellationToken);
        }
    }

    public class Validator : AbstractValidator<CreateOrderRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OrderItems).NotEmpty().WithMessage("Order items cannot be empty.");

            RuleForEach(x => x.OrderItems)
                .ChildRules(orderItem =>
                {
                    orderItem.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID cannot be null or empty.");

                    orderItem.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
                });
        }
    }
}
