using System.Security.Claims;

namespace eServices.Dms.Core.OpsPortal.Services;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		if (context.User.Identity is ClaimsIdentity identity and { IsAuthenticated: true })
		{
			using (logger.BeginScope(new ScopeData { ["Username"] = identity!.Name! }))
			{
				await next(context);
			}
		}
		else
		{
			await next(context);
		}
	}
}

public class ScopeData : Dictionary<string, object>
{
	public override string ToString()
	{
		return string.Join(", ", this.Select(x => $"{x.Key}:{x.Value}"));
	}
}
