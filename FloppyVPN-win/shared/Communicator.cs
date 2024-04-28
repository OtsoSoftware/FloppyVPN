using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace FloppyVPN
{
	public static class Communicator
	{
		public static bool IsOrchestratorAvailable()
		{
			string reply = "";

			try
			{
				reply = GetString(PathsAndLinks.orchestratorURL + "/Api/General/GetAvailability",
					out _, out _);
			}
			catch
			{
			}

			if (reply.Contains("Available."))
				return true;
			else
				return false;
		}

		public static string GetString(string url, out bool isSuccessful, out int statusCode)
		{
			try
			{
				using (HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(12) })
				{
					httpClient.DefaultRequestHeaders.Add("app_device_type", "1"); //1 = windows

					HttpResponseMessage response = httpClient.GetAsync(url).Result;

					isSuccessful = response.IsSuccessStatusCode;
					statusCode = (int)response.StatusCode;

					return httpClient.GetStringAsync(url).Result;
				}
			}
			catch (Exception ex)
			{
				isSuccessful = false;
				statusCode = 0;
				return ex.Message;
			}
		}

		public static void GetFile(string url, string path)
		{
			using (var client = new HttpClient())
			{
				using (var response = client.GetAsync(url).Result)
				{
					using (FileStream fileStream = new FileStream(path, FileMode.Create))
					{
						response.EnsureSuccessStatusCode();
						var contentStream = response.Content.ReadAsStreamAsync().Result;
						contentStream.CopyTo(fileStream);
					}
				}
			}
		}
	}
}