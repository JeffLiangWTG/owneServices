using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunity.OrgOpportunityDocManagerInfo))]
	sealed class OrgOpportunityDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return org.SalesOpportunities.AddNew();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return org.SalesOpportunities.AddNew();
		}
	}
}
