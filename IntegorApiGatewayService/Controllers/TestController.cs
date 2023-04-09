using Microsoft.AspNetCore.Mvc;

namespace IntegorApiGatewayService.Controllers
{
	[ApiController]
	[Route("test")]
	public class TestController : ControllerBase
	{
		[HttpGet]
		public IActionResult Index()
		{
			return Ok(new { test = "success" });
		}
	}
}
