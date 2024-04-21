using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
		[ServiceFilter(typeof(ClientIsBannedValidationFilter))]
		public string GetAccountData(string login)
		{
			Account account = new(login);

			if (!account.exists)
			{
				Karma karma = new(ServerTools.GetHashedIPAddress(HttpContext.Request));
				karma.LogRequest(Karma.LogRequestResources.login, false);

				HttpContext.Response.StatusCode = 403;
				return "Specified account does not exist";
			}

			Response.Headers.Add("Content-Type", "application/json");
			return $@"
				{{
					""exists"": {(account.exists ? "true" : "false")},
					""login"": ""{account.login}"",
					""date_registered"": ""{account.date_registered}"",
					""paid_till"": ""{account.paid_till}"",
					""days_left"": {account.days_left}
				}}
			";
		}

		/// <returns>
		/// List of availables country codes user has VPN configs in that belong to a user
		/// </returns>
		[HttpGet("GetCountriesList/{login}")]
		[ServiceFilter(typeof(ClientIsBannedValidationFilter))]
		public string GetCountriesList(string login)
		{
			Account account = new(login);

			if (!account.exists)
			{
				Karma karma = new(ServerTools.GetHashedIPAddress(HttpContext.Request));
				karma.LogRequest(Karma.LogRequestResources.login, false);

				HttpContext.Response.StatusCode = 403;
				return "You do not have access to these routes.";
			}

			string[] countryCodesOfAccountConfigs =
				DB.FirstColumnAsArray($@"
SELECT DISTINCT vs.country_code 
FROM vpn_servers vs 
LEFT JOIN vpn_configs vc ON vs.id = vc.server 
GROUP BY vs.country_code 
HAVING COUNT(vc.id) < vs.max_configs;
			");

			string jsonResponse = JsonConvert.SerializeObject(countryCodesOfAccountConfigs);

			Response.Headers.Add("Content-Type", "application/json");
			return jsonResponse;
		}

		/// <returns>Specific VPN config to connect to</returns>
		[HttpGet("GetConfig/{login}/{vpn_country_code}/{device_type}")]
		[ServiceFilter(typeof(ClientIsBannedValidationFilter))]
		public string GetConfig(string login, string country_code, int device_type)
		{
			Account account = new(login);
			if (!account.exists)
			{
				Karma karma = new(ServerTools.GetHashedIPAddress(HttpContext.Request));
				karma.LogRequest(Karma.LogRequestResources.login, false);

				HttpContext.Response.StatusCode = 403;
				return "Such account does not exist";
			}


			ulong config_id = Provisioner.GetConfig((ulong)account.accountData["id"], country_code, device_type);
			DataRow config = DB.GetDataTable($"SELECT * FROM `vpn_configs` WHERE `id` = {config_id};").Rows[0];
			DataRow server = DB.GetDataTable($"SELECT * FROM `vpn_servers` WHERE `id` = {config["server"]};").Rows[0];

			if (config_id != 0)
			{
				return $@"
					{{
						""country_code"": ""{server["country_code"]}"",
						""ipv4"": ""{server["ipv4_address"]}"",
						""ipv6"": ""{server["ipv6_address"]}"",
						""config"": ""{config["config"]}"",
					}}
				";
			}
			else
			{
				Response.StatusCode = 404;
				return "Could not find a suitable config.";
			}
		}
	}
}