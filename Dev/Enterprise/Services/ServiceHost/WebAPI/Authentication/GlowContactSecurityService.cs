using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Services.ServiceHost
{
	public class GlowContactSecurityService : IGlowContactSecurityService
	{
		public GlowContactSecurityService()
		{
		}

		public string AreRightsGranted(IGlowAuthenticationTicketIdentity identity, params WebSecurityRight[] rights)
		{
			if (identity == null || !identity.IsAuthenticated)
			{
				return Res.GetString("B9111EAE-C1C2-423F-8071-CC33680F1DF7", "An authentication token must be provided.");
			}

			if (identity.ProviderType == OrgContactSchema.Constants.Prefix)
			{
				var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(GlowContactSecurityService) };
				var contact = factory.Load<OrgContact>(identity.ProviderKey);
				if (contact != null)
				{
					var areRightsGranted = rights.All(right => OrgContactWebUser.IsRightGrantedWithoutCache(right, contact));

					return areRightsGranted ? null : Res.GetString("117DEE4F-2610-4088-9459-E763B175AE01", "You do not have the relevant security rights.");
				}

				return Res.GetString("64D89E24-4F65-454B-A00D-39C3C649365C", "The user record could not be found.");
			}

			return null;
		}

		public bool HasGroupRole(IGlowAuthenticationTicketIdentity identity, string groupRoleName)
		{
			if (identity == null || !identity.IsAuthenticated || identity.ProviderType != OrgContactSchema.Constants.Prefix)
			{
				return false;
			}

			var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(GlowContactSecurityService) };

			var contactLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupOrgContactLink), GlbGroupOrgContactLinkSchema.GCK_GG_Group);
			contactLinkSubQuery.AddToFilter(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, identity.ProviderKey);

			var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
			groupSubQuery.AddSubQuery(contactLinkSubQuery, JoinCondition.And);

			var securityQuery = new ZDBOnlyQuery(typeof(GlbGroupRole));
			securityQuery.AddToFilter(GlbGroupRoleSchema.GGR_RoleName, groupRoleName);
			securityQuery.AddSubQuery(GlbGroupRoleSchema.GGR_GG_Group, groupSubQuery, JoinCondition.And);

			return factory.LoadTop1<GlbGroupRole>(securityQuery) != null;
		}

		public OrgHeader GetContactOrganisation(IGlowAuthenticationTicketIdentity identity)
		{
			if (identity != null && identity.IsAuthenticated && identity.ProviderType == OrgContactSchema.Constants.Prefix)
			{
				var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(GlowContactSecurityService) };
				var contact = factory.Load<OrgContact>(identity.ProviderKey);

				if (contact != null)
				{
					return contact.ParentOrg;
				}
			}

			return null;
		}
	}
}
