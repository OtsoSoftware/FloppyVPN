using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FloppyVPN.Controllers
{
	public class AdminController : Controller
	{
		[HttpGet("/admin/{master_key}")]
		public IActionResult Index(string master_key)
		{
			Thread.Sleep(new Random().Next(100, 1000));

			if (master_key == Config.cache["master_key"].ToString())
			{
				return View();
			}
			else
			{
				return View("~/Shared/Error.cshtml");
			}
		}

		[HttpPost]
		public IActionResult AddVpnServer()
		{
			string master_key = HttpContext.Request.Form["master_key"];
			string socket = HttpContext.Request.Form["socket"];
			string country_code = HttpContext.Request.Form["country_code"];
			byte max_configs = byte.Parse(HttpContext.Request.Form["max_configs"].ToString());
			string ipv4_address = HttpContext.Request.Form["ipv4_address"];
			string ipv6_address = HttpContext.Request.Form["ipv6_address"];

			if (master_key != Config.cache["master_key"].ToString())
			{
				return Content("null");
			}

			JObject newVpnServerData = new()
			{
				{ "socket", socket },
				{ "country_code", country_code },
				{ "max_configs", max_configs },
				{ "ipv4_address", ipv4_address },
				{ "ipv6_address", ipv6_address }
			};

			string? response = Communicator.PostHttp(
				url: $"{Config.cache["orchestrator_url"]}/Api/Website/AdminAddNewVpnServer",
				body: newVpnServerData.ToString(),
				hashed_user_ip_address: ServerTools.GetHashedIPAddress(HttpContext.Request).ToString(),
				status_code: out _,
				is_successful: out _,
				master_key: Config.cache["master_key"].ToString()
			);

			return Content(response ?? "null");
		}

		[HttpPost]
		public IActionResult AliasToLogin()
		{
			string master_key = HttpContext.Request.Form["master_key"];
			string alias = HttpContext.Request.Form["alias"];

			if (master_key != Config.cache["master_key"].ToString())
			{
				return Content("null");
			}

			string? response = Communicator.GetHttp(
				url: $"{Config.cache["orchestrator_url"]}/Api/Website/GetLoginFromAlias/{alias}",
				hashed_user_ip_address: ServerTools.GetHashedIPAddress(HttpContext.Request).ToString(),
				status_code: out _,
				is_successful: out _,
				Config.cache["master_key"].ToString()
			);

			return Content(response);
		}
	}
}