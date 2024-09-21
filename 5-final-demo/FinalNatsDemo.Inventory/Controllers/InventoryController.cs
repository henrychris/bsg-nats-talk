using FinalNatsDemo.Inventory.Features.Products.GetAllProducts;
using FinalNatsDemo.Inventory.Features.Products.SetupProducts;
using HenryUtils.Api.Controllers;
using HenryUtils.Api.Responses;
using HenryUtils.Extensions;
using HenryUtils.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinalNatsDemo.Inventory.Controllers
{
    public class InventoryController(IMediator mediator) : BaseController
    {
        [HttpPost("setup")]
        [ProducesResponseType(typeof(ApiResponse<MyUnit>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetupInventoryProducts()
        {
            var result = await mediator.Send(new SetupProductsRequest());
            return result.Match(_ => Ok(result.ToSuccessfulApiResponse()), ReturnErrorResponse);
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<List<GetProductResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await mediator.Send(new GetAllProductsRequest());
            return result.Match(_ => Ok(result.ToSuccessfulApiResponse()), ReturnErrorResponse);
        }
    }
}
