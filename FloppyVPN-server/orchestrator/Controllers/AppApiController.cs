using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FloppyVPN.Controllers
{
	[ApiController]
	[Route("Api/App")]
	public class AppApiController : ControllerBase
	{
		/// <returns>Data about the account</returns>
		[HttpGet("GetAccountData/{login}")]
		[ServiceFilter(typeof(ClientIsBannedValidationFilter))]
		public string GetAccountData(string login)
		{
			Account acc = new(login);

			Response.Headers.Add("Content-Type", "application/json");
			return $@"
				{{
					""exists"": {(acc.exists ? "true" : "false")},
					""date_registered"": {acc.date_registered},
					""paid_till"": {acc.paid_till},
					""days_left"": {acc.days_left}
				}}
			";
		}

		/// <returns>List of availables country codes user has VPN configs in that belong to a user</returns>
		[HttpGet("GetCountriesList/{login}")]
		[ServiceFilter(typeof(ClientIsBannedValidationFilter))]
		public string GetCountriesList(string login)
		{
			string[] countryCodesOfAccountConfigs = 
				DB.FirstColumnAsArray("SELECT DISTINCT vs.country_code FROM vpn_servers vs " +
				"WHERE vs.id IN (SELECT vc.server FROM vpn_configs vc WHERE vc.account = @login) " +
				"ORDER BY vs.country_code ASC;",
				new Dictionary<string, object>()
				{
					{ "@login", login }
				});

			string jsonResponse = JsonConvert.SerializeObject(countryCodesOfAccountConfigs);

			Response.Headers.Add("Content-Type", "application/json");
			return jsonResponse;
		}

		/// <returns>Specific VPN config to connect to</returns>
		[HttpGet("GetConfig/{login}/{vpn_country_code}/{device_type}")]
		[ServiceFilter(typeof(ClientIsBannedValidationFilter))]
		public string GetConfig(string login, string vpn_country_code, byte device_type)
		{
			string? suitableConfig = (DB.GetValue(@"SELECT `config` FROM `vpn_configs`
				WHERE server IN(SELECT `id` FROM `vpn_servers` WHERE `country_code` = @vpn_country_code)
				AND `device_type` = @device_type AND `account` = @login;",
				new Dictionary<string, object>()
				{
					{ "@login", login },
					{ "@vpn_country_code", vpn_country_code },
					{ "@device_type", device_type },
				}) ?? "").ToString();

			if (!string.IsNullOrEmpty(suitableConfig))
			{
				return suitableConfig;
			}
			else
			{
				Response.StatusCode = 404;
				return "";
			}
		}
	}
}