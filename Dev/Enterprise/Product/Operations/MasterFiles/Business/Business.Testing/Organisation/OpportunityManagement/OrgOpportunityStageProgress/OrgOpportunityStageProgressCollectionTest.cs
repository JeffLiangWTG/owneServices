using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityStageProgressCollection))]
	sealed class OrgOpportunityStageProgressCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgOpportunityStageProgressCollection>
	{
		protected override OrgOpportunityStageProgressCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			return opp.StageProgressCollection;
		}
	}
}
