using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Api.Config;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace Enterprise.Services.Scim.Api.Auth
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
	class ApiTokenAuthHandler : AuthenticationHandler<TokenAuthenticationOptions>
	{
		const string BearerTokenPrefix = "Bearer ";

		protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			var authenticationHeaderValue = Request.Headers.Get("Authorization");

			if (authenticationHeaderValue != null && authenticationHeaderValue.StartsWith(BearerTokenPrefix))
			{
				var token = authenticationHeaderValue.Substring(BearerTokenPrefix.Length);
				if (token.Equals(SystemDataRegistry.Instance.ScimApiTokenAuthentication.Value, StringComparison.InvariantCultureIgnoreCase))
				{
					var identity = new ClaimsIdentity(Constants.TokenAuthenticationType);
					return Task.FromResult(new AuthenticationTicket(identity, new AuthenticationProperties()));
				}
			}

			return Task.FromResult(new AuthenticationTicket(null, new AuthenticationProperties()));
		}
	}
}
