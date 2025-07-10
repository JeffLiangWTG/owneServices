#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WTG.Logging.Extensions;

namespace CargoWise.Blazor.SessionBroker.Middleware
{
	public static class UserAgentCheckMiddlewareExtension
	{
		public static IApplicationBuilder UseUserAgentCheck(this IApplicationBuilder app)
		{
			if (app is null)
			{
				throw new ArgumentNullException(nameof(app));
			}
			app.UseMiddleware<UserAgentCheckMiddleware>();
			return app;
		}
	}

	public class UserAgentCheckMiddleware
	{
		const string HeaderPrefix = RequestHeaders.ClientAppUserAgentPrefix + "/";

		readonly RequestDelegate next;
		readonly ILogger<UserAgentCheckMiddleware> logger;
		readonly IHostEnvironment env;
		readonly Lazy<Version?> msixPackageVersion;

		public UserAgentCheckMiddleware(
			RequestDelegate next,
			ILogger<UserAgentCheckMiddleware> logger,
			IAppInstallerHandler appInstallerHandler,
			IHostEnvironment env)
		{
			this.next = next;
			this.logger = logger;
			this.env = env;
			msixPackageVersion = new Lazy<Version?>(() => appInstallerHandler.GetAppInstallerInfo()?.Version);
		}

		public Task InvokeAsync(HttpContext context)
		{
			// There appears to be a race condition in WebView2 where custom headers are not always added for a beacon request sent as the page is closing.
			// This means that the WTG-ClientApp-UserAgent header is not always present in the request. The purpose of this check is to prevent web browsers
			// or unsupported versions of the ClientApp from accessing an AppServer instance, so we only need to apply this check on the initial request.
			// This will either be a request made to the / or /backchannel path, as those are the routes exposed by Winzor. See WI00703177 for more details.
			var shouldCheckUserAgent = context.Request.Method == HttpMethods.Get && (context.Request.Path == "/" || context.Request.Path == "/backchannel");
			if (!shouldCheckUserAgent)
			{
				logger.LogInformation($"Skipping UserAgent check for request {context.Request.GetDisplayUrl()}.");

				return next(context);
			}

			if (!context.Request.Headers.TryGetValue(RequestHeaders.ClientAppUserAgent, out var clientAppUserAgent)
				|| !clientAppUserAgent.ToString().StartsWith(RequestHeaders.ClientAppUserAgentPrefix))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				// TEMPORARY FIX!!!
				// We have an issue where the ClientApp has not been self-updating for some users due to an issue with the MSIX installation.
				// This means that the ClientApp version is not being sent in the UserAgent header and only the old header is being sent in the request.
				// We need to detect this scenario to ensure we return the "ClientApp Version Unsupported" error rather than the "ClientApp Required" error.
				// Usually this would not be required, as the server does not need to be backwards compatable. However in this case, we will need to maintain
				// backwards compatibility temporarily to detect all the users who are unable to update and ensure they recieve the correct error message.
				if (context.Request.Headers.TryGetValue(RequestHeaders.UserAgentKeyObsolete, out var legacyClientAppUserAgent)
					&& legacyClientAppUserAgent.ToString() == RequestHeaders.ClientAppUserAgentPrefix)
				{
					// The legacy WTG-UserAgent header was included in the request instead of the new WTG-ClientApp-UserAgent header.
					// The ClientApp must have failed to self update, so perform the version check to provide a helpful error message.
					logger.LogWarning($"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request {context.Request.GetDisplayUrl()}. Only the legacy header {RequestHeaders.UserAgentKeyObsolete} was provided.");
				}
#pragma warning restore CS0618 // Type or member is obsolete
				else if (context.Request.Query.Count > 0 || context.Request.Path.Equals("/backchannel"))
				{
					// This appears to be a genuine attempt to access the app server server rather than someone visiting the root URL.
					// As there is no valid UserAgent header provided, we should block this request.
					var logInfo = !context.Request.Headers.ContainsKey(RequestHeaders.ClientAppUserAgent)
						? $"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request {context.Request.GetDisplayUrl()}."
						: $"The {RequestHeaders.ClientAppUserAgent} value {clientAppUserAgent.FirstOrDefault()} was invalid for the request {context.Request.GetDisplayUrl()}.";
					logger
						.Enrich()
						.WithAlert(LogEventCategory.IntrusionDetection)
						.LogWarning(logInfo);
					context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
					return context.Response.WriteAsync(logInfo);
				}
				else
				{
					// This is a request to the root URL, likely from a web browser as there was no valid UserAgent header provided.
					// We should forward this request to the BrowserLaunchPad page so the user can launch or download the ClientApp.
					logger.LogInformation($"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request {context.Request.GetDisplayUrl()}. Forwarding to BrowserLaunchPad.");

					context.Request.Path = "/BrowserLaunchPad";
					return next(context);
				}
			}

			// The UserAgent header is present.
			// We must also check the provided ClientApp version to ensure it's supported by the server.

			if (env.IsDevelopment())
			{
				// In development mode, we skip the version check. This allows developers to use any version of the ClientApp,
				// including a local build which doesn't have an MSIX version, without being blocked by the middleware.
				logger.LogInformation($"Skipping UserAgent version check for request {context.Request.GetDisplayUrl()} as this is a Development environment.");

				return next(context);
			}

			if (!clientAppUserAgent.ToString().StartsWith(HeaderPrefix) ||
				!Version.TryParse(clientAppUserAgent.ToString().Substring(HeaderPrefix.Length), out var clientVersion))
			{
				logger.LogError($"The {RequestHeaders.ClientAppUserAgent} value was invalid. {clientAppUserAgent}");
				context.Request.Path = "/UnsupportedClientAppVersion";
				return next(context);
			}

			if (clientVersion < msixPackageVersion.Value)
			{
				logger.LogError($"The {RequestHeaders.ClientAppUserAgent} version provided is not supported by the server. Required Version: {msixPackageVersion.Value.ToString()} Provided Version: {clientVersion.ToString()}");
				context.Request.Path = "/UnsupportedClientAppVersion";
				return next(context);
			}

			return next(context);
		}
	}
}
