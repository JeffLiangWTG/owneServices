using Enterprise.Services.Scim.Api.Config;
using Microsoft.Owin.Security;

namespace Enterprise.Services.Scim.Api.Auth
{
	public class TokenAuthenticationOptions : AuthenticationOptions
	{
		public TokenAuthenticationOptions() : base(Constants.TokenAuthenticationType)
		{
		}
	}
}
