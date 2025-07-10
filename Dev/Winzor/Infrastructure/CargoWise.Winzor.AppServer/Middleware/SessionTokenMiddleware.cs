using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTG.Logging.Extensions;

namespace CargoWise.Winzor.AppServer.Middleware;

/// <summary>
/// Validates that the CW-Session-Key matches CargoWiseAuthOptions.SessionKey
/// </summary>
public class SessionTokenMiddleware
{
	readonly RequestDelegate next;
	readonly IOptions<CargoWiseAuthOptions> options;
	readonly ILogger<SessionTokenMiddleware> logger;

	public SessionTokenMiddleware(
		RequestDelegate next,
		IOptions<CargoWiseAuthOptions> options,
		ILogger<SessionTokenMiddleware> logger)
	{
		this.next = next;
		this.options = options;
		this.logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Headers.TryGetValue(CustomHeaders.CWSessionToken, out var sessionToken)
			&& sessionToken == options.Value.SessionToken)
		{
			await next(context);
			return;
		}

		logger.Enrich()
			.WithAlert(LogEventCategory.Session, LogEventOutcome.Failure)
			.LogWarning($"The {CustomHeaders.CWSessionToken} is invalid or absent.");
		context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
	}
}
