using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSecurityCollectionForTest : OrgSecurityCollection
	{
		public OrgSecurityCollectionForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgSecurityCollectionForTest(OrgHeader organisation) : base(organisation)
		{
		}

		public OrgSecurity GetReplacementForDeletedSecurityRightExposed(OrgSecurity securityToDelete)
		{
			return GetReplacementForDeletedSecurityRight(securityToDelete);
		}
	}
}
