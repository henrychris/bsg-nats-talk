using FinalNatsDemo.Orders.Features.Orders.CreateOrder;
using HenryUtils.Api.Controllers;
using HenryUtils.Api.Responses;
using HenryUtils.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinalNatsDemo.Orders.Controllers
{
    public class OrdersController(IMediator mediator) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateOrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            var result = await mediator.Send(request);
            return result.Match(_ => Ok(result.ToSuccessfulApiResponse()), ReturnErrorResponse);
        }
    }
}
