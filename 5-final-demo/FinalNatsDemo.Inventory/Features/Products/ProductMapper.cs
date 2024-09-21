using FinalNatsDemo.Inventory.Data.Entities;
using FinalNatsDemo.Inventory.Features.Products.GetAllProducts;

namespace FinalNatsDemo.Inventory.Features.Products
{
    public static class ProductMapper
    {
        internal static GetProductResponse ToGetProductResponse(Product product)
        {
            return new GetProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                StockLevel = product.StockLevel,
                DateCreated = product.DateCreated,
                DateUpdated = product.DateUpdated,
            };
        }
    }
}
