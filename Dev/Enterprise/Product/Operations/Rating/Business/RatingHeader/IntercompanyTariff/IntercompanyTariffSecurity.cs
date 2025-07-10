using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public static class IntercompanyTariffSecurity
	{
		public static bool HasCurrentUserAccessToOrgProxy(BusinessObjectFactory factory, ZGuid orgProxyPK, string securityRight)
		{
			if (orgProxyPK.IsEmpty || IsCurrentUserControllerOrSupportUser)
			{
				return true;
			}

			var orgProxyPKs = GetAccessibleOrgProxyPKsForCurrentUser(factory, securityRight);
			return orgProxyPKs.Any(x => x.IsEmpty || x == orgProxyPK);
		}

		public static IEnumerable<ZGuid> GetAccessibleOrgProxyPKsForCurrentUser(BusinessObjectFactory factory, string securityRight)
		{
			var userSecurityRights = GetCurrentUserSecurityRights(factory, securityRight);

			var orgProxies = userSecurityRights
				.Where(x => x.GU_SecurityItemIsAllowed)
				.Select(x => x.Branch?.GB_OH_OrgProxy ?? x.Company?.GC_OH_OrgProxy ?? ZGuid.Empty)
				.ToList();

			orgProxies.AddRange(userSecurityRights
				.Where(x => x.GU_SecurityItemIsAllowed && x.Company != null && x.Branch == null)
				.SelectMany(x => x.Company.Branches.Select(b => b.GB_OH_OrgProxy))
				.Where(x => !x.IsEmpty));

			return orgProxies.Distinct();
		}

		static bool IsCurrentUserControllerOrSupportUser =>
			GlbStaff.CurrentUser.GS_IsController || GlbStaff.CurrentUser.IsSupportUser;

		static IEnumerable<GlbSecurity> GetCurrentUserSecurityRights(BusinessObjectFactory factory, string securityRight)
		{
			var userSecurityRightsQuery = new ZQuery()
				.AddToFilter(GlbSecuritySchema.GU_SecurityRight, securityRight)
				.AddToFilter(new ZQuery()
					.AddToFilter(GlbSecuritySchema.GU_GS, GlbStaff.CurrentUser.PK)
					.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GG, GlbStaff.CurrentUser.Groups.Select(x => x.PK).ToArray()));

			return factory.Load<GlbSecurity>(userSecurityRightsQuery) ?? System.Array.Empty<GlbSecurity>();
		}
	}
}
