using Microsoft.AspNetCore.Mvc;

namespace FloppyVPN.Controllers
{
	public class AdminController : Controller
	{
		[HttpGet("/admin")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
