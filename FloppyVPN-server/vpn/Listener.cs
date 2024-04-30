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

		builder.Services.Configure<KestrelServerOptions>(options =>
		{
			options.AllowSynchronousIO = true;
		});

		WebApplication app = builder.Build();

		app.Urls.Clear();
		app.Urls.Add($"http://*:{Config.Get("api_listen_port")}");



		//responds if the server is alive
		app.MapGet("/CheckAvailability", (HttpContext context) =>
		{
			return "    OK    ";
		});


		//deletes the specified config by ID
		app.MapPost("/DeleteConfig", (HttpContext context) =>
		{
			try
			{
				ulong config_id = ulong.Parse(context.DecodeBody());
				Vpn.DeleteConfig(config_id);
				return "Done".EncodeBody();
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				return $"Could not delete config: {ex.Message}".EncodeBody();
			}
		});


		//creates a config with specified ID
		app.MapPost("/CreateConfig", (HttpContext context) =>
		{
			try
			{
				ulong config_id = ulong.Parse(context.DecodeBody());
				return Vpn.CreateClientConfig(config_id);
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				return $"Could not create config: {ex.Message}".EncodeBody();
			}
		});


		//
		//app.MapGet("/", (HttpContext context) =>
		//{
		//	return "vpn";
		//});


		app.Run();
	}
}
