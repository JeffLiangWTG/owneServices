using System.Security.Claims;
using System.Security.Principal;
using CargoWise.eServices.Authentication.ServiceClient;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class AuthenticationHelper : IAuthenticationHelper
	{
		public AuthenticationHelper(IAuthWebserviceApi api, string scheme, bool disableAuthentication)
		{
			this.api = api;
			this.scheme = scheme;
			this.disableAuthentication = disableAuthentication;
		}
		readonly string scheme;
		readonly bool disableAuthentication;
		readonly IAuthWebserviceApi api;

		public ClaimsPrincipal GetClaimsPrincipal(string authHeader)
		{
			ClaimsPrincipal result = null;
			var credentials = RequestHelper.GetUserIdAndPassword(authHeader);
			if (credentials != null)
			{
				var isValid = api == null ? true : api.ValidateSystem(credentials.Item1, credentials.Item2);
				if (disableAuthentication || isValid)
				{
					result = new ClaimsPrincipal(new GenericIdentity(credentials.Item1, scheme));
				}
			}
			return result;
		}
	}
}
