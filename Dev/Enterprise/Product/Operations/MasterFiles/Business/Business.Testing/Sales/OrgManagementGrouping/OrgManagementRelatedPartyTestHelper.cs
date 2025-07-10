using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class OrgManagementRelatedPartyTestHelper
	{
		public static OrgManagementRelatedParty Create(BusinessObjectFactory factory, OrgHeader parentOrganisation, OrgHeader subsidiaryOrganisation, GlbCompany company = null)
		{
			var relatedParty = factory.New<OrgManagementRelatedParty>();
			relatedParty.PR_OH_Parent = subsidiaryOrganisation.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_GC = company != null ? company.PK : ZGuid.Empty;
			relatedParty.PR_OH_RelatedParty = parentOrganisation.PK;
			return relatedParty;
		}
	}
}
