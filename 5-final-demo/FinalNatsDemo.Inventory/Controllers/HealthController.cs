using HenryUtils.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FinalNatsDemo.Inventory.Controllers;

public class HealthController : BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello World!");
    }
}
