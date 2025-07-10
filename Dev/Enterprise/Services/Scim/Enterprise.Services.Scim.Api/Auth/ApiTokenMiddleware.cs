using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace Enterprise.Services.Scim.Api.Auth
{
	public class ApiTokenMiddleware : AuthenticationMiddleware<TokenAuthenticationOptions>
	{
		public ApiTokenMiddleware(OwinMiddleware next, TokenAuthenticationOptions options) : base(next, options)
		{
		}

		protected override AuthenticationHandler<TokenAuthenticationOptions> CreateHandler()
		{
			return new ApiTokenAuthHandler();
		}
	}
}
