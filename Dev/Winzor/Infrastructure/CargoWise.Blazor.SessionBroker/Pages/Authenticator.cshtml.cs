using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWiseNext.Infrastructure.Authentication;
using Enterprise.Integration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CargoWise.Blazor.SessionBroker.Pages
{
	public class AuthenticatorModel : PageModel
	{
		readonly ILogger<AuthenticatorModel> logger;
		readonly IAuthenticationService authenticationService;
		readonly IAuthenticationConfigAccessor registryAccessor;
		public AuthenticatorModel(
			ILogger<AuthenticatorModel> logger,
			IAuthenticationService authenticationService,
			IAuthenticationConfigAccessor registryAccessor)
		{
			this.logger = logger;
			this.authenticationService = authenticationService;
			this.registryAccessor = registryAccessor;
		}

		public IActionResult OnGet()
		{
			return Page();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string ConstructLoginMessage()
		{
			try
			{
				logger.LogInformation("Fetching OIDC config");
				var oidcConfig = registryAccessor.GetOIDCConfig();

				if (oidcConfig == null)
				{
					var errorString = "OIDC login configuration not found";
					logger.LogWarning(errorString);
					return authenticationService.BuildJsonFailedLoginMessage(errorString);
				}

				logger.LogInformation("Validating OIDC Config settings");

				var validatingOidcConfig = ValididatingOidcConfig(oidcConfig);

				if (!validatingOidcConfig.isValid)
				{
					logger.LogWarning(validatingOidcConfig.oidcConfigError);
					return authenticationService.BuildJsonFailedLoginMessage("OIDC login configuration is invalid");
				}

				logger.LogInformation("Fetching domain hint");

				var defaultDomainHint = registryAccessor.GetDomainHint();
				if (!HttpContext.Request.Query.TryGetValue(QueryParameters.LoginPrompt, out var loginPrompt))
				{
					loginPrompt = "Default";
				}

				logger.LogInformation("Building the login message");
				return authenticationService.BuildJsonLoginMessage(
					clientId: oidcConfig.ClientIdentifier,
					identityProvider: oidcConfig.OIDCServerType.ToString(),
					authority: new Uri(oidcConfig.AuthorityURL),
					loginPrompt: loginPrompt,
					domainHint: defaultDomainHint,
					redirectUrl: BuildRedirectUri(),
					additionalScopes: FlatOIDCScopes(oidcConfig.Scopes));
			}
			catch (DatabaseAccessException ex)
			{
				logger.LogWarning(ex, "Failed to query OIDC settings from database.");
				return authenticationService.BuildJsonFailedLoginMessage(ex.Message);
			}
			catch (Exception ex)
			{
				var errorString = "Failed to build the login message";
				logger.LogWarning(ex, errorString);
				return authenticationService.BuildJsonFailedLoginMessage(errorString);
			}
		}

		internal static IEnumerable<string> FlatOIDCScopes(IEnumerable<IOIDCScope> scopes)
		{
			if (scopes == null || !scopes.Any())
			{
				return Array.Empty<string>();
			}

			return scopes.Where(scope => scope != null && !scope.ScopeName.IsEmpty)
						 .Select(scope => scope.ScopeName.ToString())
						 .ToArray();
		}

		internal Uri BuildRedirectUri()
		{
			var redirectBuilder = new UriBuilder($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/");
			var qs = HttpContext.Request.Query.ContainsKey(QueryParameters.NetCore) ? new QueryString($"?{QueryParameters.NetCore}") : new QueryString();
			if (HttpContext.Request.Query.TryGetValue(QueryParameters.Persist, out var persist))
			{
				qs = qs.Add(QueryParameters.Persist, persist);
			}
			redirectBuilder.Query = qs.ToString();

			return redirectBuilder.Uri;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		(bool isValid, string oidcConfigError) ValididatingOidcConfig(IOIDCConfig oidcConfig)
		{
			var errorBuilder = new StringBuilder();
			var isValid = true;

			if (!oidcConfig.IsOIDCEnabled)
			{
				errorBuilder.AppendLine("OIDC is not enabled. Login will not be completed");
				isValid = false;
			}
			else if (string.IsNullOrEmpty(oidcConfig.AuthorityURL) && string.IsNullOrEmpty(oidcConfig.ClientIdentifier))
			{
				errorBuilder.AppendLine("OIDC is enabled, but no valid authority or client identifier is defined. Please check your OIDC config settings and try again. Login will not be completed");
				isValid = false;
			}
			else if (string.IsNullOrEmpty(oidcConfig.AuthorityURL))
			{
				errorBuilder.AppendLine("OIDC is enabled, but no valid authority URL is defined. Login will not be completed");
				isValid = false;
			}
			else if (string.IsNullOrEmpty(oidcConfig.ClientIdentifier))
			{
				errorBuilder.AppendLine("OIDC is enabled, but no valid client identifier is defined. Login will not be completed");
				isValid = false;
			}

			var oidcConfigError = errorBuilder.ToString().Trim();
			return (isValid, oidcConfigError);
		}
	}
}
