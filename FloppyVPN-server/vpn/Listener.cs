using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Data;
using System.Text;

namespace FloppyVPN;

/// <summary>
/// A tiny API that listens for master server commands
/// </summary>
public static class Listener
{
	public static void Start()
	{
		var builder = WebApplication.CreateBuilder();

		var app = builder.Build();

		builder.Services.Configure<KestrelServerOptions>(options =>
		{
			options.AllowSynchronousIO = true;
		});

		app.Urls.Clear();
		app.Urls.Add("http://*:1513");

		app.UseMiddleware<DecodeRequestFilter>();


		//responses if the server is alive
		app.MapGet("/CheckAvailability", (HttpContext context) =>
		{
			return "    OK    ".EncodeAsResponse();
		});


		//deletes the specified config
		app.MapDelete("/DeleteConfig", (HttpContext context) =>
		{
			string config_to_delete = ;
			Vpn.DeleteConfig(config_to_delete);
			return "Done".EncodeAsResponse();
		});


		//replaces existing config
		app.MapGet("/CreateConfig", (HttpContext context) =>
		{
			try
			{
				return Vpn.CreateConfig();
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				return ("Could not create config: " + ex.Message).EncodeAsResponse();
			}
		});


		//
		app.MapGet("/", () =>
		{
			return "vpn";
		});




		app.Run();
	}
}


public class DecodeRequestFilter : IActionFilter
{
	public async void OnActionExecuting(ActionExecutingContext context)
	{
		if (!ServerTools.IsValidMasterKey(context.HttpContext.Request))
		{
			string requestBody;
			using (StreamReader sr = new(context.HttpContext.Request.Body))
			{
				requestBody = await sr.ReadToEndAsync();
			}

			string transformedBody = Cryption.De(requestBody, Program.masterKey);

			context.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(transformedBody));
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}

public static class EncodeResponseExtention
{
	public static string EncodeAsResponse(this string response)
	{
		return Cryption.En(response, Program.masterKey);
	}
}