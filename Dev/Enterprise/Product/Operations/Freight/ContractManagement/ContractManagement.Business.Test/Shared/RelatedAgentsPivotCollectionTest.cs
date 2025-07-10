using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(RelatedAgentsPivotCollection))]
	public class RelatedAgentsPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<RelatedAgentsPivotCollection>
	{
		protected override RelatedAgentsPivotCollection GetCollectionToTest()
		{
			return new RelatedAgentsPivotCollection(Factory.NewWithValidTestData<RatingContractAllocationLine>());
		}

		public void TestGetAllAgents()
		{
			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			allocationRoute.AgentPivots.AddRelatedIfNotExist(org1);
			var agents = allocationRoute.AgentPivots.GetAllAgents().Cast<OrgHeader>().ToList();
			AssertEquals(1, agents.Count);
			AssertContainsExactElementsInAnyOrder([org1], agents);

			allocationRoute.AgentPivots.AddRelatedIfNotExist(org2);
			agents = allocationRoute.AgentPivots.GetAllAgents().Cast<OrgHeader>().ToList();
			AssertEquals(2, agents.Count);
			AssertContainsExactElementsInAnyOrder([org1, org2], agents);
		}
	}
}
