using FinalNatsDemo.Inventory.Data;
using HenryUtils.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Inventory.Features.Products.GetAllProducts
{
    public class GetAllProductsRequest : IRequest<Result<List<GetProductResponse>>> { }

    public class Handler(InventoryDataContext context, ILogger<Handler> logger)
        : IRequestHandler<GetAllProductsRequest, Result<List<GetProductResponse>>>
    {
        public async Task<Result<List<GetProductResponse>>> Handle(GetAllProductsRequest request, CancellationToken cancellationToken)
        {
            var productResponse = await context.Products.Select(x => ProductMapper.ToGetProductResponse(x)).ToListAsync();

            logger.LogInformation("Fetched {count} product(s)", productResponse.Count);
            return Result<List<GetProductResponse>>.Success(productResponse);
        }
    }
}
