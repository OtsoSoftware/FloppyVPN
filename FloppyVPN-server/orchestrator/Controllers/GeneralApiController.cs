using Microsoft.AspNetCore.Mvc;

namespace FloppyVPN.Controllers
{
	[ApiController]
	[Route("Api/General")]
	public class GeneralApiController : ControllerBase
	{
		[HttpGet("GetAvailability")]
		public string GetAvailability()
		{
			return "Available.";
		}

		[HttpGet("CurrentDateTime")]
		public string CurrentDateTime()
		{
			return DateTime.Now.ToDateTime();
		}
	}
}