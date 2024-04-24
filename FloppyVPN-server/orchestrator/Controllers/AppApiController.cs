using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FloppyVPN.Controllers
{
	/// <summary>
	/// Uses json to communicate with client apps
	/// </summary>
	[ApiController]
	[Route("Api/App")]
	public class AppApiController : ControllerBase
	{
		/// <returns>Data about the account</returns>
		[HttpGet("GetAccountData/{login}")]
		[ServiceFilter(typeof(BannedClientsFilter))]
		public IActionResult GetAccountData(string login)
		{
			Account account = new(login);

			if (!account.exists)
			{
				Karma karma = new(ServerTools.GetHashedIPAddress(HttpContext.Request));
				karma.LogRequest(Karma.LogRequestResources.login, false);

				HttpContext.Response.StatusCode = 403;
				return Content("Specified account does not exist");
			}

			Response.Headers.Add("Content-Type", "application/json");

			JObject accountInfo = new()
			{
				["exists"] = account.exists,
				["login"] = account.login,
				["date_registered"] = account.date_registered,
				["paid_till"] = account.paid_till,
				["days_left"] = account.days_left
			};

			return Content(accountInfo.ToString(), "application/json");
		}

		/// <returns>
		/// List of availables country codes user has VPN configs in that belong to a user
		/// </returns>
		[HttpGet("GetCountriesList/{login}")]
		[ServiceFilter(typeof(BannedClientsFilter))]
		public IActionResult GetCountriesList(string login)
		{
			Account account = new(login);

			if (!account.exists)
			{
				Karma karma = new(ServerTools.GetHashedIPAddress(HttpContext.Request));
				karma.LogRequest(Karma.LogRequestResources.login, false);

				HttpContext.Response.StatusCode = 403;
				return Content("You do not have access to these routes.");
			}

			string[] countryCodesAvailable = DB.FirstColumnAsArray($@"
SELECT DISTINCT vs.country_code
FROM vpn_servers vs
LEFT JOIN (
	SELECT server, COUNT(*) AS config_count
	FROM vpn_configs
	GROUP BY server
) AS vc ON vs.id = vc.server
WHERE (vc.config_count IS NULL OR vc.config_count < vs.max_configs);
			");

			string jsonResponse = JsonConvert.SerializeObject(countryCodesAvailable);

			Response.Headers.Add("Content-Type", "application/json");
			return Content(jsonResponse, "application/json");
		}

		/// <returns>Specific VPN config to connect to</returns>
		[HttpGet("GetConfig/{login}/{vpn_country_code}/{device_type}")]
		[ServiceFilter(typeof(BannedClientsFilter))]
		public IActionResult GetConfig(string login, string country_code, int device_type)
		{
			Account account = new(login);
			if (!account.exists)
			{
				Karma karma = new(ServerTools.GetHashedIPAddress(HttpContext.Request));
				karma.LogRequest(Karma.LogRequestResources.login, false);

				HttpContext.Response.StatusCode = 403;
				return Content("Such account does not exist");
			}


			ulong config_id = Provisioner.GetConfig((ulong)account.accountData["id"], country_code, device_type);
			DataRow config = DB.GetDataTable($"SELECT * FROM `vpn_configs` WHERE `id` = {config_id};").Rows[0];
			DataRow server = DB.GetDataTable($"SELECT * FROM `vpn_servers` WHERE `id` = {config["server"]};").Rows[0];

			if (config_id != 0)
			{

				JObject configInfo = new()
				{
					["country_code"] = server["country_code"].ToString(),
					["ipv4"] = server["ipv4_address"].ToString(),
					["ipv6"] = (server["ipv6_address"] ?? "").ToString(),
					["config"] = config["config"].ToString()
				};

				Response.Headers.Add("Content-Type", "application/json");
				return Content(configInfo.ToString(), "application/json");
			}
			else
			{
				Response.StatusCode = 404;
				return Content("Could not find a suitable config.");
			}
		}
	}
}