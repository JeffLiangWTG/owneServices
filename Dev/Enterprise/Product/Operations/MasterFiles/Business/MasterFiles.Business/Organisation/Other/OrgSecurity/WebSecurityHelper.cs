using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class WebSecurityExtensions
	{
		public static bool IsRightGranted(this OrgContact contact, WebSecurityRight right)
		{
			return WebSecurityHelper.IsRightGranted(contact.Factory, contact.PK, contact.OC_OH, right);
		}

		public static bool IsRightGrantedWithCheckingSecurityGroups(this OrgContact contact, WebSecurityRight right)
		{
			if (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value)
			{
				if (right.SecurityGuid.IsEmpty)
				{
					var contactLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupOrgContactLink), GlbGroupOrgContactLinkSchema.GCK_GG_Group);
					contactLinkSubQuery.AddToFilter(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, contact.PK);

					var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
					groupSubQuery.AddSubQuery(contactLinkSubQuery, JoinCondition.And);

					var securityQuery = new ZDBOnlyQuery(typeof(GlbSecurity));
					securityQuery.AddToFilter(GlbSecuritySchema.GU_SecurityRight, right.SecurityItemName);
					securityQuery.AddSubQuery(GlbSecuritySchema.GU_GG, groupSubQuery, JoinCondition.And);

					return contact.Factory.LoadTop1<GlbSecurity>(securityQuery) != null;
				}
				else
				{
					var contactLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupOrgContactLink), GlbGroupOrgContactLinkSchema.GCK_GG_Group);
					contactLinkSubQuery.AddToFilter(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, contact.PK);

					var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
					groupSubQuery.AddSubQuery(contactLinkSubQuery, JoinCondition.And);

					var securityQuery = new ZDBOnlyQuery(typeof(GlbSecurity));
					securityQuery.AddToFilter(GlbSecuritySchema.GU_ItemGUID, right.SecurityGuid);
					securityQuery.AddSubQuery(GlbSecuritySchema.GU_GG, groupSubQuery, JoinCondition.And);

					return contact.Factory.LoadTop1<GlbSecurity>(securityQuery) != null;
				}
			}

			return contact.IsRightGranted(right);
		}
	}

	public static class WebSecurityHelper
	{
		public static bool IsRightGranted(BusinessObjectFactory factory, ZGuid contactPK, ZGuid contactOrgPK, WebSecurityRight right)
		{
			var orgRightQuery = new ZQuery(OrgSecuritySchema.OX_OH, contactOrgPK);
			if (right.SecurityGuid.IsEmpty)
			{
				orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SecurityItemName, right.SecurityItemName);
			}
			else
			{
				orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SU, right.SecurityGuid);
			}

			var orgRight = factory.LoadTop1<OrgSecurity>(orgRightQuery);
			if (orgRight == null)
			{
				return right.IsGrantedByDefault;
			}

			var contactRightQuery = new ZQuery(OrgSecurityContactsSchema.OZ_OC, contactPK)
				.AddToFilter(OrgSecurityContactsSchema.OZ_OX, orgRight.PK);

			var contactRight = factory.LoadTop1<OrgSecurityContacts>(contactRightQuery);

			return contactRight?.OZ_Granted ?? orgRight.OX_Granted;
		}
	}
}
