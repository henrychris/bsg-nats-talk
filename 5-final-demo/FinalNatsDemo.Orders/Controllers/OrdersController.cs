using FinalNatsDemo.Common;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Orders.Features.Orders.CreateOrder;
using HenryUtils.Api.Controllers;
using HenryUtils.Api.Responses;
using HenryUtils.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinalNatsDemo.Orders.Controllers
{
    public class OrdersController(IMediator mediator, INatsWrapper natsWrapper) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateOrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            var result = await mediator.Send(request);
            return result.Match(_ => Ok(result.ToSuccessfulApiResponse()), ReturnErrorResponse);
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestPublish()
        {
            await natsWrapper.PublishAsync(new Message(), "test.message");
            return Ok();
        }
    }
}
