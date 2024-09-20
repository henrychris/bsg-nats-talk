using HenryUtils.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FinalNatsDemo.Orders.Controllers;

public class HealthController : BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello World!");
    }
}
