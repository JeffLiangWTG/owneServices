using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Enterprise.Services.Scim.Api.Config;
using Enterprise.ZArchitecture.Core;
using Microsoft.Owin;

namespace Enterprise.Services.Scim.Api.Middlewares
{
	public class ThrottlingMiddleware(OwinMiddleware next, IAppSettings appSettings) : OwinMiddleware(next)
	{
		readonly int ThrottleTime = appSettings.ThrottleTimeInSeconds;
		readonly int MaxRequestCount = appSettings.ThrottleMaxRequestCount;

		public override async Task Invoke(IOwinContext context)
		{
			var response = context.Response;
			var clientIp = context.Request.Headers["X-Forwarded-For"]?.Split([',']).LastOrDefault()?.Trim()
				?? context.Request.RemoteIpAddress;

			var allowedIps = appSettings.ThrottleSkippedIps.Split(',');
			if (string.IsNullOrEmpty(clientIp) || allowedIps.Contains(clientIp))
			{
				await Next.Invoke(context);
				return;
			}

			var count = 1;

			if (HttpRuntime.Cache[clientIp] != null)
			{
				count = (int)HttpRuntime.Cache[clientIp] + 1;
			}

			if (count > MaxRequestCount)
			{
				response.OnSendingHeaders(state =>
				{
					var resp = state as IOwinResponse;
					resp.Headers.Add("Retry-After", [ThrottleTime.ToString()]);
					resp.StatusCode = 429;
					resp.ReasonPhrase = (NoResString)"Too many requests";
				}, response);
				return;
			}
			else
			{
				HttpRuntime.Cache.Insert(clientIp, count, null, DateTime.UtcNow.AddSeconds(ThrottleTime), Cache.NoSlidingExpiration, CacheItemPriority.Low, null);
			}

			await Next.Invoke(context);
		}
	}
}
