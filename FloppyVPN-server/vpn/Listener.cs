using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Data;
using System.Text;
using System;

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
		app.MapPost("/CreateConfig", (HttpContext context) =>
		{
			try
			{
				ulong config_id = context.Request.Body.read
				return Vpn.CreateClientConfig();
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
