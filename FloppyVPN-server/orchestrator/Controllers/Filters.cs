using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FloppyVPN.Controllers;

public class Filters
{
	/// <summary>
	/// To be used to obtain user's hashed IP address when communicating indirectly (ex. via website server)
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	public static string GetHashedIpFromHeaders(HttpRequest request)
	{
		try
		{
			return request.Headers["hashed_user_ip_address"].FirstOrDefault();
		}
		catch
		{
			return "";
		}
	}
}

public class BannedUsersFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{

		Karma userKarma = new(Filters.GetHashedIpFromHeaders(context.HttpContext.Request));
		if (userKarma.IsBanned())
		{
			userKarma.LogRequest(Karma.LogRequestResources.idk, false);

			context.HttpContext.Response.StatusCode = StatusCodes.Status423Locked;
			context.HttpContext.Response.WriteAsync("The user seems to be banned right now.");
			context.Result = new EmptyResult();
		}
		else
		{
			userKarma.LogRequest(Karma.LogRequestResources.idk, true);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}

public class SoftbannedUsersFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		Karma userKarma = new(Filters.GetHashedIpFromHeaders(context.HttpContext.Request));
		if (userKarma.IsSoftBanned() || userKarma.IsBanned())
		{
			context.HttpContext.Response.StatusCode = 403;
			context.HttpContext.Response.WriteAsync("The user seems to be softbanned right now.");
			context.Result = new EmptyResult();
		}
		else
		{
			userKarma.LogRequest(Karma.LogRequestResources.registration, true);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}


/// <summary>
/// Use when communicating directly from client to server. User's hashed ip will be obtained directly
/// </summary>
public class BannedClientsFilter : IActionFilter
{
	Karma userKarma;

	public void OnActionExecuting(ActionExecutingContext context)
	{
		userKarma = new(ServerTools.GetHashedIPAddress(context.HttpContext.Request));
		if (userKarma.IsBanned())
		{
			context.HttpContext.Response.StatusCode = 403;
			context.HttpContext.Response.WriteAsync("The user seems to be banned right now.");
			context.Result = new EmptyResult();
		}
		else
		{
			userKarma.LogRequest(Karma.LogRequestResources.registration, true);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}

public class MasterKeyValidationFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		Karma serverKarma = new(ServerTools.GetHashedIPAddress(context.HttpContext.Request));

		if (serverKarma.IsBanned())
		{
			context.HttpContext.Response.StatusCode = 403;
			context.HttpContext.Response.WriteAsync("You seem to be banned.");
			context.Result = new EmptyResult();
		}
		else if (!ServerTools.IsValidMasterKey(context.HttpContext.Request))
		{
			serverKarma.LogRequestAsync(Karma.LogRequestResources.master_key_failed_usage, false);

			context.HttpContext.Response.StatusCode = 401;
			context.HttpContext.Response.WriteAsync("Master key is wrong or absent.");
			context.Result = new EmptyResult();
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}
