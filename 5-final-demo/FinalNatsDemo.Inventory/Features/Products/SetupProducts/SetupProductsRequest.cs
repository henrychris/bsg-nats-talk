using FinalNatsDemo.Common.Events;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Inventory.Data;
using FinalNatsDemo.Inventory.Data.Entities;
using HenryUtils.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Inventory.Features.Products.SetupProducts
{
    public class SetupProductsRequest : IRequest<Result<MyUnit>> { }

    public class Handler(InventoryDataContext context, INatsWrapper natsWrapper, ILogger<Handler> logger)
        : IRequestHandler<SetupProductsRequest, Result<MyUnit>>
    {
        private readonly List<Product> _products =
        [
            new Product
            {
                Id = "ca25fda2-7ec4-4bbd-b0af-63173a39020e",
                Name = "Nike Football",
                StockLevel = 2000000,
            },
            new Product
            {
                Id = "32c722e1-84ab-439f-a582-448cf430bcd8",
                Name = "Blue Notebook",
                StockLevel = 2000000,
            },
            new Product
            {
                Id = "9abe38a4-733e-42af-8a60-7cc75283a1bd",
                Name = "Adidas Football",
                StockLevel = 2000000,
            },
        ];

        public async Task<Result<MyUnit>> Handle(SetupProductsRequest request, CancellationToken cancellationToken)
        {
            List<Product> createdProducts = [];

            foreach (var product in _products)
            {
                var doesProductExist = await context.Products.AnyAsync(x => x.Id == product.Id, cancellationToken: cancellationToken);
                if (doesProductExist)
                {
                    logger.LogWarning("Product already exists. Name: {productName}. Skipping...", product.Name);
                    continue;
                }

                await context.AddAsync(product, cancellationToken);
                createdProducts.Add(product);
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Saved products to database");

            await PublishProductCreatedEvent(createdProducts);
            return Result<MyUnit>.Success(MyUnit.Value);
        }

        private async Task PublishProductCreatedEvent(List<Product> createdProducts)
        {
            foreach (var product in createdProducts)
            {
                await natsWrapper.PublishToJetStreamAsync(ProductMapper.ToProductCreatedEvent(product), ProductCreatedEvent.TOPIC, product.Id);
            }
        }
    }
}
