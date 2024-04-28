using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FloppyVPN
{
	internal static class Account
	{
		public static bool loggedIn = false;
		public static string login = "";
		public static string masked_login = "";
		public static string paid_till = "";
		public static int days_left = -1;


		public static bool LogIn(string _login = null) //if no login provided, try using saved one
		{
			if (string.IsNullOrEmpty(_login))
				_login = IniFile.GetValue("login");
			if (string.IsNullOrEmpty(_login))
				return false;

			string _response = Communicator.GetString($"{PathsAndLinks.orchestratorURL}/Api/App/GetAccountData/{_login}",
				out _, out _);

			JObject response = JObject.Parse(_response);
			
			if (!(bool)response["exists"])
				return false;

			loggedIn = true;
			login = (string)response["login"];
			masked_login = GetMaskedLogin(login);
			paid_till = ((DateTime)response["paid_till"]).ToShortDateString();
			days_left = (int)response["days_left"];


			IniFile.SetValue("login", login);

			return true;
		}

		public static void LogOut()
		{
			loggedIn = false;
			login = "";
			masked_login = "";
			paid_till = "";
			days_left = -1;

			IniFile.SetValue("login", "");
		}

		private static string GetMaskedLogin(string originalLogin)
		{
			string[] parts = originalLogin.Split('-');
			string part1 = parts[0];
			string part2 = string.Concat(Enumerable.Repeat("*", parts[1].Length));

			return string.Join("-", part1, part2);
		}
	}
}