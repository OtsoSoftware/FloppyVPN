using Microsoft.AspNetCore.Http;
using System.Net;

namespace FloppyVPN
{
	/// <summary>
	/// For making HTTP requests for communication between servers easily.
	/// </summary>
	public static class Communicator
	{
		/// <summary>
		/// Sends an HTTP GET request.
		/// </summary>
		/// <param name="url">The URL to send the request to.</param>
		/// <param name="master_key">The value of the "master_key" header.</param>
		/// <returns>The response body.</returns>
		public static string GetHttp(string url, string hashed_user_ip_address, out HttpStatusCode status_code, out bool is_successful, string master_key = "")
		{
			using (HttpClient client = new())
			{
				client.DefaultRequestHeaders.Add("master_key", master_key);
				client.DefaultRequestHeaders.Add("hashed_user_ip_address", hashed_user_ip_address);

				HttpResponseMessage response = client.GetAsync(url).Result;

				status_code = response.StatusCode;
				is_successful = response.IsSuccessStatusCode;

				return response.Content.ReadAsStringAsync().Result;
			}
		}

		/// <summary>
		/// Sends an HTTP POST request.
		/// </summary>
		/// <param name="url">The URL to send the request to.</param>
		/// <param name="body">The body of the request.</param>
		/// <param name="master_key">The value of the "master_key" header.</param>
		/// <returns>The response body.</returns>
		public static string PostHttp(string url, string body, string master_key, string hashed_user_ip_address, out HttpStatusCode status_code, out bool is_successful)
		{
			using (HttpClient client = new())
			{
				client.DefaultRequestHeaders.Add("master_key", master_key);
				client.DefaultRequestHeaders.Add("hashed_user_ip_address", hashed_user_ip_address);

				HttpContent content = new StringContent(body, System.Text.Encoding.UTF8);
				HttpResponseMessage response = client.PostAsync(url, content).Result;

				status_code = response.StatusCode;
				is_successful = response.IsSuccessStatusCode;

				return response.Content.ReadAsStringAsync().Result;
			}
		}

		public static string? SimpleGetHttp(string url)
		{
			using (HttpClient client = new())
			{
				HttpResponseMessage response = client.GetAsync(url).Result;
				return response.Content.ReadAsStringAsync().Result;
			}
		}
	}

	/// <summary>
	/// Simple yet handy extentions to encode responses and decode requests
	/// </summary>
	public static class EncodedCommunicationExtensions
	{
		public static string DecodeBody(this HttpContext context)
		{
			string? requestBody = null;

			using (StreamReader sr = new(context.Request.Body))
			{
				requestBody = sr.ReadToEnd();
			}

			return Cryption.De(requestBody, Config.cache["master_key"].ToString());
		}

		public static string EncodeBody(this string response)
		{
			return Cryption.En(response, Config.cache["master_key"].ToString());
		}
	}
}