using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	public interface IGlowContactSecurityService
	{
		string AreRightsGranted(IGlowAuthenticationTicketIdentity identity, params WebSecurityRight[] rights);

		bool HasGroupRole(IGlowAuthenticationTicketIdentity identity, string groupRoleName);

		OrgHeader GetContactOrganisation(IGlowAuthenticationTicketIdentity identity);
	}
}
