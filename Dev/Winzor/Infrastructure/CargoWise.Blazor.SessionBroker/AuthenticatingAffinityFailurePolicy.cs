using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Pages;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WTG.Logging.Extensions;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.SessionAffinity;
using RequestHeaders = CargoWise.Blazor.Client.Integration.RequestHeaders;

namespace CargoWise.Blazor.SessionBroker
{
	public class AuthenticatingAffinityFailurePolicy : IAffinityFailurePolicy
	{
		readonly IOptions<CargoWiseOptions> cargoWiseOptions;
		readonly AppInstanceService appInstanceService;
		readonly IDataProtector dataProtector;
		readonly IOptions<AffinityCookieOptionsProvider> affinityCookieProviderOptions;
		readonly ILogger<AuthenticatingAffinityFailurePolicy> logger;
		readonly ICargoWiseAuthenticator cargoWiseAuthenticator;
		readonly IServiceProvider serviceProvider;
		readonly IMemoryCache memoryCache;

		public AuthenticatingAffinityFailurePolicy(
			IOptions<CargoWiseOptions> cargoWiseOptions,
			AppInstanceService appInstanceService,
			IOptions<AffinityCookieOptionsProvider> affinityCookieProviderOptions,
			IDataProtectionProvider dataProtectionProvider,
			ILogger<AuthenticatingAffinityFailurePolicy> logger,
			ICargoWiseAuthenticator cargoWiseAuthenticator,
			IServiceProvider serviceProvider,
			IMemoryCache memoryCache)
		{
			this.cargoWiseOptions = cargoWiseOptions;
			this.appInstanceService = appInstanceService;
			this.affinityCookieProviderOptions = affinityCookieProviderOptions;
			this.logger = logger;
			dataProtector = dataProtectionProvider.CreateProtector("Yarp.ReverseProxy.SessionAffinity.CookieSessionAffinityPolicy");
			this.cargoWiseAuthenticator = cargoWiseAuthenticator;
			this.serviceProvider = serviceProvider;
			this.memoryCache = memoryCache;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal const string PolicyName = "Authenticate";

		public string Name => PolicyName;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public async Task<bool> Handle(HttpContext context, ClusterState cluster, AffinityStatus affinityStatus)
		{
			logger.LogInformation("AuthenticatingAffinityFailurePolicy.Handle");

			try
			{
				(string Server, string Database) dbDetails = (cargoWiseOptions.Value.DbServerName, cargoWiseOptions.Value.DatabaseName);

				var cargoWiseAuthCookieValue = context.Request.Cookies[OidcConstants.CargoWiseAuthCookieName];
				var clientAppSessionHeaderValue = context.Request.Headers[RequestHeaders.ClientAppIdentifier].SingleOrDefault();
				var clientToken = context.Request.Query[QueryParameters.ClientToken].SingleOrDefault();
				var persist = context.Request.Query[QueryParameters.Persist].SingleOrDefault();
				var isNetCore = context.Request.Query.ContainsKey(QueryParameters.NetCore);
				if (!string.IsNullOrWhiteSpace(clientToken))
				{
					if (cargoWiseAuthenticator.ValidateClientToken(clientToken))
					{
						var id = appInstanceService.CreateWithStmAccessToken($"{dbDetails.Server} {dbDetails.Database}", clientToken, clientAppSessionHeaderValue);
						ManuallySetAffinityCookie(id, context);
						context.Response.Redirect(GetRedirectUri(context.Request.Path, context.Request.QueryString));
					}
					else
					{
						logger.Enrich()
							.WithAlert(LogEventCategory.Authentication, LogEventOutcome.Failure)
							.LogWarning("Invalid client token");
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					}
				}
				else if (!string.IsNullOrWhiteSpace(cargoWiseAuthCookieValue))
				{
					var cargoWiseAuthCookie = JsonConvert.DeserializeObject<CargoWiseAuthCookie>(cargoWiseAuthCookieValue);

					logger.LogInformation("Deleting CargoWise auth cookie");
					context.Response.Cookies.Delete("CargoWiseAuthCookie");

					logger.LogInformation($"dbDetails: {dbDetails.Server} {dbDetails.Database}");

					switch (await cargoWiseAuthenticator.VerifyAccessTokenAsync(cargoWiseAuthCookie, context.RequestAborted))
					{
						case VerificationResult.Success:
							var id = appInstanceService.CreateWithOIDCAuthCookie(
								databaseConfigHeader: $"{dbDetails.Server} {dbDetails.Database}",
								cargoWiseAuthCookie: cargoWiseAuthCookie,
								clientIdentifier: clientAppSessionHeaderValue,
								persist: persist,
								isNetCore: isNetCore);
							ManuallySetAffinityCookie(id, context);
							context.Response.Redirect(GetRedirectUri(context.Request.Path, context.Request.QueryString));
							break;

						case VerificationResult.AuthorisationFailed:
							logger.LogWarning("Authorisation failed");
							context.Response.ContentType = "text/html";
							context.Response.StatusCode = StatusCodes.Status200OK;
							if (!memoryCache.TryGetValue("LoginFailedPage", out string loginFailedPageResponse))
							{
								loginFailedPageResponse = (await MessageBoxPromptAsync<LoginFailed>());

								memoryCache.Set("LoginFailedPage", loginFailedPageResponse);
							}
							await context.Response.WriteAsync(loginFailedPageResponse);
							break;

						case VerificationResult.SiteOffline:
							logger.LogWarning("Site Offline");
							context.Response.ContentType = "text/html";
							context.Response.StatusCode = StatusCodes.Status200OK;
							await context.Response.WriteAsync(await MessageBoxPromptAsync<SiteOffline>());
							break;

						case VerificationResult.GeneralFailure:
							logger.Enrich()
								.WithEvent(LogEventCategory.Authentication, LogEventOutcome.Failure)
								.LogWarning("Invalid or expired jwt token");
							RedirectToAuthEndpoint(context);
							break;
					}
				}
				else
				{
					RedirectToAuthEndpoint(context);
				}

				return false;
			}
			catch (InvalidDatabaseConfigurationException ex)
			{
				logger.LogError(ex, $"Invalid database configuration");
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				return false;
			}
		}

		void RedirectToAuthEndpoint(HttpContext context)
		{
			if (context.Request.Path.ToString() != "/")
			{
				//We only redirect request come from root path.
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}
			var qs = context.Request.Query.ContainsKey(QueryParameters.NetCore) ? new QueryString($"?{QueryParameters.NetCore}") : new QueryString();
			if (context.Request.Query.TryGetValue(QueryParameters.LoginPrompt, out var loginPrompt))
			{
				qs = qs.Add(QueryParameters.LoginPrompt, loginPrompt);
			}
			if (context.Request.Query.TryGetValue(QueryParameters.Persist, out var persist))
			{
				qs = qs.Add(QueryParameters.Persist, persist);
			}

			context.Response.Redirect($"/_sessionbroker/auth{qs}");
		}

		static string GetRedirectUri(string path, QueryString queryString)
		{
			var query = QueryHelpers.ParseQuery(queryString.ToString());
			query.Remove(QueryParameters.NetCore);
			query.Remove(QueryParameters.ClientToken);
			query.Remove(QueryParameters.Persist);
			return new Uri(QueryHelpers.AddQueryString(path, query), UriKind.RelativeOrAbsolute).ToString();
		}

		// Previously, we had some middleware that would adjust the reverse proxy destinations to try and influence YARP to set an affinity cookie
		// targeting the app server process that we had launched. However, YARP changed their approach for session affinity (in preview 9), which
		// means that this no longer works. To work around, we now manually set the cookie using code adapted from YARP's cookie session affinity provider
		// See https://github.com/microsoft/reverse-proxy/blob/v1.0.0-preview11/src/ReverseProxy/Service/SessionAffinity/BaseSessionAffinityProvider.cs
		// and https://github.com/microsoft/reverse-proxy/blob/v1.0.0-preview11/src/ReverseProxy/Service/SessionAffinity/CookieSessionAffinityProvider.cs
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ManuallySetAffinityCookie(string id, HttpContext context)
		{
			logger.LogInformation("Setting affinity cookie");
			var bytes = Encoding.UTF8.GetBytes(id);
			var cookieValue = Convert.ToBase64String(dataProtector.Protect(bytes)).TrimEnd('=');
			var cookieOptions = affinityCookieProviderOptions.Value.Cookie.Build(context);
			context.Response.Cookies.Append(affinityCookieProviderOptions.Value.Cookie.Name, cookieValue, cookieOptions);
		}

		async Task<string> MessageBoxPromptAsync<TPage>() where TPage : IComponent
		{
			using (var scope = serviceProvider.CreateScope())
			{
				// Use the service provider from the scope to resolve HtmlRenderer
				var htmlRenderer = scope.ServiceProvider.GetRequiredService<HtmlRenderer>();
				var htmlAsString = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
				{
					var output = await htmlRenderer.RenderComponentAsync<TPage>();

					return output.ToHtmlString();
				});

				return htmlAsString;
			}
		}
	}
}
