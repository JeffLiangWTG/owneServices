using System.Security.Principal;
namespace Enterprise.Services.ServiceHost.WebAPI.Authentication
{
	public static class GlowPrincipalHelper
	{
		public static bool IsStaff(IPrincipal user) => user?.Identity is IGlowAuthenticationTicketIdentity identity && identity.IsStaff();
	}
}
