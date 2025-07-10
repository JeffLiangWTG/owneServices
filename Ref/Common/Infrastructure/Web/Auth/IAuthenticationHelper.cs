using System.Security.Claims;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public interface IAuthenticationHelper
	{
		ClaimsPrincipal GetClaimsPrincipal(string authHeader);
	}
}
