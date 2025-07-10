using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityTradeLanePivotCollection))]
	sealed class OrgOpportunityTradeLanePivotCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgOpportunityTradeLanePivotCollection>
	{
		protected override OrgOpportunityTradeLanePivotCollection GetCollectionToTest()
		{
			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			return new OrgOpportunityTradeLanePivotCollection(opportunity);
		}
	}
}
