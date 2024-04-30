using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FloppyVPN
{
	/// <summary>
	/// Represents current connection config for VPN
	/// </summary>
	internal static class Conf
	{
		/// <summary>
		/// Validness of current config. Basically - can we connect or not
		/// </summary>
		public static bool IsValid { get; private set; }
		public static string CurrentCountryCode { get; set; }
		public static string IPv4Address { get; private set; }
		public static string IPv6Address { get; private set; }
		public static string ConfString { get; private set; }



		/// <summary>
		/// Attempts to obtain and put a new config.
		/// </summary>
		/// <returns>true if successfully obtained and put and false if failed</returns>
		public static bool Obtain(string vpn_country_code)
		{
			string _response = Communicator.GetString(
				$"{PathsAndLinks.orchestratorURL}/Api/App/GetConfig/{Account.login}/{vpn_country_code}/1",
				out bool isSuccessful,
				out _);

			if (isSuccessful)
			{
				JObject response = JObject.Parse(_response);

				CurrentCountryCode = (string)response["country_code"];
				IPv4Address = (string)response["ipv4"];
				IPv6Address = (string)response["ipv6"];
				ConfString = (string)response["config"];

				IsValid = true;
				return true;
			}
			else
			{
				new MsgBox(_response, Loc.errorConnectingCaption);
				IsValid = false;
				return false;
			}
		}

		public static JArray GetAvailableCountryCodes()
		{
			string _response = Communicator.GetString(
				$"{PathsAndLinks.orchestratorURL}/Api/App/GetAvailableCountryCodes/{Account.login}/{Loc.lang}",
				out bool isSuccessful,
				out int statusCode);

			if (!isSuccessful)
				throw new Exception($"Failed to get locations list:{statusCode}");

			return JArray.Parse(_response);
		}
	}
}